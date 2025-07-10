using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DeferredAmendmentSavingOptions : Customs.Business.DeferredAmendmentSavingOptions, Customs.Business.IDeferredAmendmentSavingOptions
	{
		public DeferredAmendmentSavingOptions(JobDeclaration declaration)
			: base()
		{
			this.declaration = declaration;
			if (this.declaration != null)
			{
				foreach (ZPropertyInfo crucialField in GetCrucialFields())
				{
					SignificantAmendments = crucialField.HasChanges;
					if (SignificantAmendments)
					{
						break;
					}
				}
			}
		}

		ZPropertyInfo[] GetCrucialFields()
		{
			return new ZPropertyInfo[] { declaration.JE_MergeByInfo };
		}

		readonly JobDeclaration declaration;

		#region IDeferredAmendmentSavingOptions Members

		public void ProcessWhenChangesAreSavedWithoutSending(CargoWise.Types.ZString amendmentReason)
		{
			if (declaration != null)
			{
				if (SaveWithoutEntryChanges)
				{
					RevertEntriesToDBValuesToMatchCustomsEntries();
					declaration.OutstandingAmendmentLogManger.AddANewLogForSaveWithoutEntryChanges();
				}
				else if (SaveWithEntryChanges)
				{
					declaration.OutstandingAmendmentLogManger.AddANewOutstandingAmendmentLog(amendmentReason);
				}
			}
		}

		void RevertEntriesToDBValuesToMatchCustomsEntries()
		{
			if (declaration.IsInDatabase)
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				foreach (CusEntryHeader currentEntryHeader in declaration.CustomsEntryHeaders)
				{
					if (currentEntryHeader.IsInDatabase && currentEntryHeader.IsStatusPostLodge)
					{
						CusEntryHeader dBEntryHeader = (CusEntryHeader)newFactory.Load(typeof(CusEntryHeader), currentEntryHeader.PK);

						if (dBEntryHeader != null)
						{
							currentEntryHeader.CH_TotalPaid = dBEntryHeader.CH_TotalPaid;
							currentEntryHeader.Charges.CopyChargesValuesFrom(dBEntryHeader.Charges);

							foreach (CusEntryLine currentEntryLine in currentEntryHeader.MergedLines)
							{
								if (currentEntryLine.IsInDatabase)
								{
									CusEntryLine dBEntryLine = (CusEntryLine)newFactory.Load(typeof(CusEntryLine), currentEntryLine.PK);
									if (dBEntryLine != null)
									{
										currentEntryLine.CL_CustomsValue = dBEntryLine.CL_CustomsValue;
										currentEntryLine.EntryLineAddInfo.ZA_TILV = dBEntryLine.EntryLineAddInfo.ZA_TILV;
										currentEntryLine.Fees.CopyChargesValuesFrom(dBEntryLine.Fees);
									}
									else
									{
										var directDbCount = Factory.GetDatabaseCount(typeof(CusEntryLine), new ZQuery(CusEntryLineSchema.PK, currentEntryLine.PK));
										ErrorReporter.ReportOnce("DeferredAmendmentSavingOptions.RevertEntriesToDBValuesToMatchCustomsEntries--DBEntryHeader!=null", "EntryLine is in Database, but loading in a different factory results in null. EntryLine:" + currentEntryLine.PK + "  IsDeleted:" + currentEntryLine.IsDeleted + " DB count with PK:" + directDbCount); // Column name used in error message, not key
									}
								}
							}
						}
						else
						{
							var directDbCount = Factory.GetDatabaseCount(typeof(CusEntryHeader), new ZQuery(CusEntryHeaderSchema.PK, currentEntryHeader.PK));
							ErrorReporter.ReportOnce("DeferredAmendmentSavingOptions.RevertEntriesToDBValuesToMatchCustomsEntries--DBEntryHeader==null", "Entry is in Database, but loading in a different factory results in null. Entry:" + currentEntryHeader.PK + "  IsDeleted:" + currentEntryHeader.IsDeleted + " DB count with PK:" + directDbCount); // Column name used in error message, not key
						}
					}
				}
			}
		}

		#endregion
	}
}
