using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.DFD.Business.Import
{
	class USOrganisationDataImporter : DataImporter
	{
		#region Override

		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			additionalTransactionActions = System.Array.Empty<ITransactionParticipant>();

			var customsPartyOrg = FactoryProvider.Current.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "00001142");
			if (customsPartyOrg == null)
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, "Unable to find Customs Agent Broker organisation with code 00001142"));
				return false;
			}

			var errorBuilder = new ZStringBuilder();
			int numberOfRowsToSave = 0;

			string line = dataReader.ReadLine();
			while ((line = dataReader.ReadLine()) != null)
			{
				if (FindOrgAndUpdate(line, errorBuilder, customsPartyOrg))
				{
					numberOfRowsToSave++;
				}

				if (numberOfRowsToSave % 500 == 0)
				{
					FactoryProvider.SaveCurrentAndCreateNew();
				}
			}

			if (!errorBuilder.IsEmpty)
			{
				notifications.AddError(string.Format("Unable not match organisation with following details provided: {0}{1}", System.Environment.NewLine, errorBuilder.ToStringWithNewLineBetweenAppends()));
			}

			if (numberOfRowsToSave.Equals(ZInt.Zero))
			{
				notifications.AddError("No records to save found.");
				return false;
			}
			else
			{
				notifications.Notify(new InfoNotification(string.Format("{0} organisations updated.", numberOfRowsToSave)));
			}

			return true;
		}

		#endregion

		#region Implementation

		bool FindOrgAndUpdate(string line, ZStringBuilder errorBuilder, OrgHeader customsPartyOrg)
		{
			var dataRow = new USOrganisationDataRow(new FlatFileDataRow(new OCsvLine(line).FieldValues));
			bool matchedByLegacyCode = true;
			var org = FindOrganisationByLegacyCode(dataRow.LegacyCode);
			if (org == null)
			{
				org = FindOrganisationByEIN(dataRow.EIN);
				matchedByLegacyCode = false;
			}
			if (org == null)
			{
				errorBuilder.Append(string.Format("Legacy Code: {0} EIN: {1}", dataRow.LegacyCode, dataRow.EIN));
				return false;
			}
			else
			{
				var wrapper = OrgHeaderWrapper.New(org);
				var oneBondDetail = Get_1BondDetail(wrapper);
				oneBondDetail.PW_ActivityCode = ActivityCodeList.Codes._1;
				oneBondDetail.PW_BondType = dataRow.BondType;
				oneBondDetail.PW_BondNumber = dataRow.BondNumber;
				oneBondDetail.PW_BondAmount = dataRow.BondAmount;
				oneBondDetail.PW_SuretyCode = dataRow.SuretyCode;
				oneBondDetail.PW_BondEffectiveDate = ZDateTime.Now;

				wrapper.ZO_PaymentType = dataRow.CustomsStatementType.IsEmpty ? PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode : dataRow.CustomsStatementType.ToString();
				wrapper.ZO_AccountNo = dataRow.ACHPayerUnit;
				org.OH_IsConsignee = true;

				var customsParties = org.AllRelatedParties.Cast<OrgRelatedParty>().Where(party => party.PR_PartyType == RelatedPartyTypeList.Codes.CustomsAgentBroker
					&& party.PR_FreightDirection.Equals(RelatedPartyDirectionList.Codes.PickupAndDelivery)
					&& party.PR_FreightTransportMode.Equals(Core.Constants.TransportModes.All));
				if (customsParties.Count().Equals(ZInt.Zero))
				{
					var customsParty = org.AllRelatedParties.AddNew();
					customsParty.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
					customsParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.PickupAndDelivery;
					customsParty.PR_FreightTransportMode = Core.Constants.TransportModes.All;
					customsParty.PR_OH_RelatedParty = customsPartyOrg.PK;
				}
				else
				{
					customsParties.First().PR_OH_RelatedParty = customsPartyOrg.PK;
				}

				var docQuery = new ZQuery(JobRequiredDocumentSchema.EQ_ParentID, org.PK);
				docQuery.AddToFilter(JobRequiredDocumentSchema.EQ_DocCategory, Core.Constants.ReferenceTypes.ClientSupplierRelationship);
				docQuery.AddToFilter(JobRequiredDocumentSchema.EQ_DocType, Core.Constants.RefDocTypes.PowerOfAttorney);
				var requiredDocs = FactoryProvider.Current.Load<JobRequiredDocument>(docQuery);
				if (requiredDocs.Length.Equals(ZInt.Zero))
				{
					var requiredDoc = FactoryProvider.Current.New<JobRequiredDocument>();
					requiredDoc.EQ_ParentID = org.PK;
					requiredDoc.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
					requiredDoc.ParentType = typeof(OrgHeader);
					requiredDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
					requiredDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
					requiredDoc.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
					requiredDoc.EQ_DateReceived = ZDateTimeOffset.Now;
					requiredDoc.EQ_ValidToDate = ZDateTime.Now.AddYears(5);
					requiredDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
					requiredDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.UnitedStates;
				}
				else
				{
					requiredDocs.First().ParentType = typeof(OrgHeader);
					requiredDocs.First().EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
					requiredDocs.First().EQ_DateReceived = ZDateTimeOffset.Now;
					requiredDocs.First().EQ_ValidToDate = ZDateTime.Now.AddYears(5);
					requiredDocs.First().EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
					requiredDocs.First().EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.UnitedStates;
				}

				if (matchedByLegacyCode && FindOrganisationByEIN(dataRow.EIN) == null && !dataRow.EIN.IsEmpty)
				{
					string codeType = string.Empty;
					switch (dataRow.EIN.IndexOf("-"))
					{
						case 2: codeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber; break;
						case 6: codeType = OrgCusCode.USACodeTypes.CBPAssignedNumber; break;
						case 3: codeType = OrgCusCode.USACodeTypes.SocialSecurityNumber; break;
					}
					if (!string.IsNullOrEmpty(codeType))
					{
						org.CustomsCodes.AddNew(codeType, dataRow.EIN, RefCountry.LoadFromCountryCode(FactoryProvider.Current, Core.Constants.CountryCodes.UnitedStates));
					}
				}

				return true;
			}
		}

		Customs.US.Business.CusBondDetail Get_1BondDetail(OrgHeaderWrapper orgWrapper)
		{
			var oneboundDetails = orgWrapper.BondDetails.Find(new ZQuery(CusBondDetailSchema.PW_ActivityCode, ActivityCodeList.Codes._1));
			if (oneboundDetails.Length > 0)
			{
				return oneboundDetails.Cast<Customs.US.Business.CusBondDetail>().First();
			}
			else
			{
				return orgWrapper.BondDetails.AddNew();
			}
		}

		#region Find Organisation

		OrgHeader FindOrganisationByLegacyCode(ZString legacyCode)
		{
			if (!legacyCode.IsEmpty)
			{
				var cusCode = new OrgCusCode.Loader(FactoryProvider.Current).LoadFromLegacyCode(Core.Constants.CountryCodes.UnitedStates, legacyCode);
				if (cusCode != null)
				{
					return cusCode.Header;
				}
			}

			return null;
		}

		OrgHeader FindOrganisationByEIN(ZString eIN)
		{
			if (!eIN.IsEmpty)
			{
				string codeType = string.Empty;
				switch (eIN.IndexOf("-"))
				{
					case 2: codeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber; break;
					case 6: codeType = OrgCusCode.USACodeTypes.CBPAssignedNumber; break;
					case 3: codeType = OrgCusCode.USACodeTypes.SocialSecurityNumber; break;
				}
				var cusCode = new OrgCusCode.Loader(FactoryProvider.Current).Load(Core.Constants.CountryCodes.UnitedStates, codeType, eIN);
				if (cusCode != null && cusCode.Length > 0)
				{
					return cusCode[0].Header;
				}
			}

			return null;
		}

		#endregion

		#endregion
	}
}
