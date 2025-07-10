using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.BISI
{
	/// <summary>
	/// BISI Downloader - File Data Importer
	/// </summary>
	public class PWSFileImporter
	{
		public bool Import(string fileName, INotifications notifications)
		{
			bool result;

			try
			{
				using (PWSRecordFileReader reader = new PWSRecordFileReader(fileName, notifications))
				{
					result = reader.IsValid;
					if (result)
					{
						foreach (PWSRecordBase record in reader)
						{
							PWSTotalDataAreaRecord dataRecord = record as PWSTotalDataAreaRecord;
							if (dataRecord != null)
							{
								ImportToFactory(dataRecord);
							}
						}
						Factory.Save();
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notifications.AddError(ex.Message);
				result = false;
			}

			return result;
		}

		#region Implementation

		protected virtual void ImportToFactory(PWSTotalDataAreaRecord record)
		{
			ZQuery hawbFilter = GetHAWBFilter(record);

			if (hawbFilter != null)
			{
				BusinessObjectFactory readOnlyFactory = new BusinessObjectFactory();
				Callout callout = (Callout)readOnlyFactory.LoadTop1(typeof(Callout), hawbFilter);
				if (callout != null)
				{
					callout = Factory.Load<Callout>(callout.PK);
					IBisiDownload bisiShipment = callout;
					if (bisiShipment.TransferredDateTime.IsEmpty)
					{
						callout.EnsureJobHeaderExists();
						foreach (PWSChargeDetails charge in record.Charges)
						{
							callout.JobHeader.Charges.AddNewFromPWSChargeDetails(charge);
						}
						callout.CalculateTotalAmountDue();
						if (!record.DimensionalWeight.IsEmpty)
						{
							callout.CS_ChargableWeight = record.DimensionalWeight;
						}
						callout.BillToAccountNumber = (record.BillToAccount.IsEmpty && callout.ImporterOrConsigneeMatchedOrgPK.IsValid) ? GetImporterOrConsigneeAccountNumber(callout) : record.BillToAccount;
						callout.InvoiceNumber = record.InvoiceNumber;
						bisiShipment.TransferredDateTime = ZDateTime.Now;  // should actually be .DbServerNow
						bisiShipment.OnAfterBisiDownload();  // This functionality was called (aka hidden) from both Callout.BillToAccountNumber & Callout.BisiDownloadDate properties.  The latter one now extinct.
					}

					if (Globals.IsTest)
					{
						ImportedCalloutsForTest.Add(callout);
					}

					SaveAndResetCounterIfReachesMaxImportCount();
				}
				else
				{
					ImportTheChargesIntoPWSTable(record, readOnlyFactory);
				}
			}
		}

		void ImportTheChargesIntoPWSTable(PWSTotalDataAreaRecord record, BusinessObjectFactory factory)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(ClientPWSHeader));

			if (!record.WayBillNumber.IsEmpty)
			{
				filter.AddToFilter(ClientPWSHeaderSchema.U1_WayBillNumber, SQLComparisonOperator.Equal, record.WayBillNumber);
			}

			if (!record.WayBillShortNumber.IsEmpty)
			{
				filter.AddToFilter(JoinCondition.Or, ClientPWSHeaderSchema.U1_WayBillShortNumber, SQLComparisonOperator.Equal, record.WayBillShortNumber);
			}

			ClientPWSHeader header = factory.LoadTop1<ClientPWSHeader>(filter);

			if (header == null)
			{
				header = Factory.New<ClientPWSHeader>();
				header.U1_WayBillNumber = record.WayBillNumber;
				header.U1_WayBillShortNumber = record.WayBillShortNumber;
				header.U1_InvoiceNumber = record.InvoiceNumber;
				header.U1_BillableWeight = record.DimensionalWeight;
				header.U1_BillToAccount = record.BillToAccount;
				header.U1_ImportedDate = ZDateTime.Now;
			}

			ClientPWSCharge bisiCharge;

			foreach (PWSChargeDetails pwsCharge in record.Charges)
			{
				if (header.Charges.FindByChargeDescription(pwsCharge.ChargeDescription) == null)
				{
					bisiCharge = header.Charges.AddNew();
					bisiCharge.U2_ChargeDescription = pwsCharge.ChargeDescription;
					bisiCharge.U2_TaxableAmount = pwsCharge.TaxableAmount;
					bisiCharge.U2_NonTaxableAmount = pwsCharge.NonTaxableAmount;
					bisiCharge.U2_Discount = pwsCharge.Discount;
					bisiCharge.U2_NetAmount = pwsCharge.NettAmount;
				}
			}
		}

		ZString GetImporterOrConsigneeAccountNumber(Callout callout)
		{
			UPEOrgHeader importerOrConsigneeMatchedOrg = callout.Factory.Load<UPEOrgHeader>(callout.ImporterOrConsigneeMatchedOrgPK);
			return importerOrConsigneeMatchedOrg.AccountNumber;
		}

		internal Callout[] GetLastImportedCalloutsForTest()
		{
			return (Callout[])ImportedCalloutsForTest.ToArray(typeof(Callout));
		}

		ArrayList ImportedCalloutsForTest
		{
			get
			{
				if (fImportedCalloutsForTest == null)
				{
					fImportedCalloutsForTest = new ArrayList();
				}
				return fImportedCalloutsForTest;
			}
		}

		ArrayList fImportedCalloutsForTest;

		ZQuery GetHAWBFilter(PWSTotalDataAreaRecord record)
		{
			ZQuery result = null;

			if (!record.WayBillNumber.IsEmpty)
			{
				result = GetFilterFromFullTrackingNumber(record.WayBillNumber);
			}
			else if (!record.WayBillShortNumber.IsEmpty)
			{
				result = GetFilterFromShortTrackingNumber(record.WayBillShortNumber);
			}

			return result;
		}

		ZQuery GetFilterFromFullTrackingNumber(ZString wayBillNumber)
		{
			return new ZQuery(CusHAWBSchema.CS_HAWB, wayBillNumber);
		}

		ZQuery GetFilterFromShortTrackingNumber(ZString wayBillShortNumber)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(UPECusHAWB));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(UPEJobRelatedWayBill), JobRelatedWayBillSchema.EB_ParentID);
			subQuery.AddToFilter(JobRelatedWayBillSchema.EB_WaybillShortNumber, wayBillShortNumber);
			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		void SaveAndResetCounterIfReachesMaxImportCount()
		{
			FactorySaveCounter++;
			if (FactorySaveCounter >= MaxFactorySaveCounter)
			{
				Factory.Save();
				FactorySaveCounter = 0;
			}
		}

		BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}

		BusinessObjectFactory fFactory;
		int FactorySaveCounter;
		readonly int MaxFactorySaveCounter = 50;

		#endregion
	}
}
