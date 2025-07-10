using System.IO;
using System.Runtime.InteropServices;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUImportTariffBulkChange : TariffBulkChange, IObsoleteValidation
	{
		public AUImportTariffBulkChange(BusinessObjectFactory factory)
			: base(factory, BaseCusClassification.ClassificationType.IMP)
		{
		}

		public override ZString ReferenceKey => "HS2022 TARIFF " + lookupType;

		public override ZGuid CountryPK => Core.Constants.CountryGuids.Australia;

		public override ZString CountryCode => Enterprise.Core.Constants.CountryCodes.Australia;

		protected override TariffFormatter GetCurrentTariffFormatter() => new AUImportTariffFormatter();

		protected override ZQuery GetClassificationQuery(ZString oldtariffnum)
		{
			var dummyOldTariff = new TariffBulkChangeOldTariff(this);
			dummyOldTariff.OldTariffNum = oldtariffnum;

			var classificationQuery = new ZQuery();
			classificationQuery.AddToFilter(CusClassificationSchema.CC_ClassificationType, lookupType);
			classificationQuery.AddToFilter(CusClassificationSchema.CC_TariffNum, SQLComparisonOperator.StartsWith, dummyOldTariff.TariffNumNoStats);
			classificationQuery.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			if (dummyOldTariff.StatsCode != "")
			{
				classificationQuery.AddToFilter(CusClassificationSchema.CC_TariffNum, SQLComparisonOperator.EndsWith, dummyOldTariff.StatsCode);
			}

			return classificationQuery;
		}

		protected override void ProcessOne2One(ZString oldtariffnum, ZString firstnewtariffnum)
		{
			var formattedNewTariff = CurrentTariffFormatter.DisplayFormat(firstnewtariffnum);
			var filter = GetClassificationQuery(oldtariffnum);
			var oldTariffClassificationCollection = new TBCClassificationCollection<TBCClassification>(Factory, filter);
			oldTariffClassificationCollection.Load();

			foreach (TBCClassification classification in oldTariffClassificationCollection)
			{
				var originTariffNum = classification.CC_TariffNum;
				classification.NewTariffNum = originTariffNum.Length <= formattedNewTariff.Length || formattedNewTariff.Length > 10
						? formattedNewTariff
						: (ZString)(formattedNewTariff + originTariffNum.Substring(formattedNewTariff.Length));
			}
		}

		public ContinueWithSave TCOAdditionalContinueWithSave()
		{
			var result = ContinueWithSave.No;
			string messageText = "";
			if (!IsProductionDataBase)
			{
				if (IsDateInvalid)
				{
					messageText = DateWarningMessage;
					result = ContinueWithSave.Yes;
				}
				else
				{
					result = ContinueWithSave.Yes;
				}
			}
			else if (IsDemoCompany)
			{
				Globals.Message.ShowError(DemoBranchMessage, "Access Denied");// Message is valid and required here
			}
			else
			{
				if (!IsSaveAllowed)
				{
					Globals.Message.ShowError(SaveNotAllowedMessage, SaveNotAllowedResourceString);// Message is valid and required here
				}
				else
				{
					result = ContinueWithSave.Yes;
				}
			}
			if (result == ContinueWithSave.Yes)
			{
				if (!string.IsNullOrEmpty(messageText))
				{
					messageText += " ";
				}
				messageText += "Your Data Base will now be updated with TCO changes, Do you wish to continue?";
				if (Globals.Message.Show(messageText, "Final Confirmation", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question) != ZDialogResult.Yes)// Message is valid and required here
				{
					result = ContinueWithSave.No;
				}
			}
			return result;
		}

		string[] cSVcolumns;
		readonly char[] delimiters = { ',' };

		public void ChangeTCO(Stream str, [Optional] bool isSaveAllowed)
		{
			IsSaveAllowed = isSaveAllowed;
			if (str != null)
			{
				using (str)
				{
					var reader = new StreamReader(str);
					while (reader.Peek() >= 0)
					{
						cSVcolumns = reader.ReadLine().Split(delimiters, 3);
						if (cSVcolumns.Length == 3)
						{
							ZString oldInstrumentCode = cSVcolumns[0];
							ZString tariffNum = cSVcolumns[1];
							ZString newInstrumentCode = cSVcolumns[2];
							if (!oldInstrumentCode.IsEmpty && !tariffNum.IsEmpty)
							{
								var classificationFilter = new ZQuery();
								classificationFilter.AddToFilter(CusClassificationSchema.CC_ClassificationType, lookupType);
								classificationFilter.AddToFilter(CusClassificationSchema.CC_TariffNum, SQLComparisonOperator.StartsWith, tariffNum);
								classificationFilter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, Enterprise.Core.Constants.CountryCodes.Australia);
								var oldTCOClassifications = new BaseClassificationCollection<Classification>(Factory, classificationFilter);
								oldTCOClassifications.Load();

								newInstrumentCode = newInstrumentCode.Left(8);
								foreach (Classification classification in oldTCOClassifications)
								{
									if (classification.InstrumentType.Left(2) == "TC" && classification.InstrumentCode == oldInstrumentCode)
									{
										classification.InstrumentCode = newInstrumentCode;
									}

									var pivotFilter = new ZQuery();
									pivotFilter.AddToFilter(CusClassPartPivotSchema.CI_CC, classification.PK);
									pivotFilter.AddToFilter(CusClassPartPivotSchema.CI_AddInfo, SQLComparisonOperator.Contains, "InstrumentType_Hidden=TC");
									pivotFilter.AddToFilter(CusClassPartPivotSchema.CI_AddInfo, SQLComparisonOperator.Contains, "InstrumentCode_Hidden=" + oldInstrumentCode);
									var pivots = Factory.Load<CusClassPartPivot>(pivotFilter);
									foreach (CusClassPartPivot pivot in pivots)
									{
										pivot.AddInfo.ZA_InstrumentCode_Hidden = newInstrumentCode;
									}
								}
							}
						}
					}
				}
				str.Dispose();
			}
		}
	}
}
