using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	class CDSJobComInvoiceLineValueSetStrategy : JobComInvoiceLineValueSetStrategy
	{
		public CDSJobComInvoiceLineValueSetStrategy(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected override string SupportingDocumentTypeForValueMethodOne => EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N935;

		public override void HandleSettingOfNewValuationMethod()
		{
			// See tariff V3 page 3-10. 
			switch (invoiceLine.JI_ValuationCode)
			{
				case ValuationMethodList.Codes._1:
				case ValuationMethodList.Codes._2:
				case ValuationMethodList.Codes._3:
					CreateSupportingDocumentForValueMethodOne();
					break;
				case ValuationMethodList.Codes._4:
				case ValuationMethodList.Codes._5:
				case ValuationMethodList.Codes._6:
					CreateSupportingDocument(base.doc9WKS, invoiceLine.Declaration.JE_DeclarationReference, string.Format("SEE ATTACHED WORKSHEET {0}", invoiceLine.Declaration.JE_DeclarationReference));
					DeleteSupportingDocument(SupportingDocumentTypeForValueMethodOne);
					break;

				default:
					DeleteSupportingDocument(base.doc9WKS);
					DeleteSupportingDocument(SupportingDocumentTypeForValueMethodOne);
					break;
			}
		}

		protected override void CreateSupportingDocumentForValueMethodOne()
		{
			MeasuresToTaxAndDocsHelper.FindExistingSupportingDocument(SupportingDocumentTypeForValueMethodOne, invoiceLine?.InvoiceHeader, invoiceLine?.InvoiceHeader?.JZ_InvoiceNumber ?? ZString.Empty, out var suppDoc);
			if (suppDoc == null)
			{
				CreateSupportingDocument(SupportingDocumentTypeForValueMethodOne, invoiceLine?.InvoiceHeader?.JZ_InvoiceNumber);
			}
		}

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);
			switch (valueThatHasChanged.Name)
			{
				case JobComInvoiceLine.Schema.JI_CustomsSecondQuantity:
				case JobComInvoiceLine.Schema.JI_CustomsThirdQuantity:
				case JobComInvoiceLine.Schema.JI_CustomsFourthQuantity:
				case JobComInvoiceLine.Schema.JI_CustomsFifthQuantity:
					AutoConvertExciseUnits(valueThatHasChanged.Name, (ZDecimal)valueThatHasChanged.Value);
					break;
			}
		}

		void DeleteSupportingDocument(string sdType)
		{
			MeasuresToTaxAndDocsHelper.FindExistingSupportingDocument(sdType, invoiceLine, out SupportingDocument sd);
			sd?.Delete();
		}

		#region Excise units conversion

		void AutoConvertExciseUnits(ZString propertyName, ZDecimal newValue)
		{
			var canSetProperty = GBCustomsDataRegistry.Instance.CDSEnableAutoConvertExciseUnits.Value && newValue > 0 && HasExciseCode && !IsImportingData &&
				!invoiceLine.IsUpdatingDetailsFromPart && !invoiceLine.SetterSuspender.IsSetterSuspended(propertyName);
			if (canSetProperty)
			{
				var quantities = new[]
				{
					new { Name = JobComInvoiceLine.Schema.JI_CustomsSecondQuantity, Quantity = invoiceLine.JI_CustomsSecondQuantity, Unit = invoiceLine.JI_CustomsSecondUnitQty },
					new { Name = JobComInvoiceLine.Schema.JI_CustomsThirdQuantity, Quantity = invoiceLine.JI_CustomsThirdQuantity, Unit = invoiceLine.JI_CustomsThirdUnitQty },
					new { Name = JobComInvoiceLine.Schema.JI_CustomsFourthQuantity, Quantity = invoiceLine.JI_CustomsFourthQuantity, Unit = invoiceLine.JI_CustomsFourthUnitQty },
					new { Name = JobComInvoiceLine.Schema.JI_CustomsFifthQuantity, Quantity = invoiceLine.JI_CustomsFifthQuantity, Unit = invoiceLine.JI_CustomsFifthUnitQty }
				};
				var quantitiesWithAlcohol = quantities.Where(p => IsAutoConvertableUnit(p.Unit)).ToArray();
				var sources = quantitiesWithAlcohol.Where(p => p.Quantity > 0).ToArray();
				var targets = quantitiesWithAlcohol.Where(p => p.Quantity == 0).ToArray();
				if (sources.Length >= 2 && targets.Length > 0)
				{
					foreach (var target in targets)
					{
						var equivalentSource = sources.Where(s => AreUnitsEquivalent(s.Unit, target.Unit)).FirstOrDefault();
						var quantity = equivalentSource?.Quantity ?? ConvertExciseUnit(sources[0].Quantity, sources[0].Unit, sources[1].Quantity, sources[1].Unit, target.Unit);
						if (quantity > 0)
						{
							SetCustomsQuantity(target.Name, quantity);
						}
					}
				}
			}
		}

		bool IsImportingData => ((ISupportDataImporting)invoiceLine).IsImportingData;

		bool HasExciseCode => invoiceLine.SupplementaryCodes.Any(sc => sc.CY_Code.StartsWith("X"));

		bool IsAutoConvertableUnit(ZString unit) => unit == "LTR" || unit == "LPA" || unit == "ASV" || unit == "ASVX";

		bool AreUnitsEquivalent(ZString unit1, ZString unit2) => (unit1 == "ASVX" && unit2 == "LPA") || (unit1 == "LPA" && unit2 == "ASVX");

		ZDecimal ConvertExciseUnit(ZDecimal value1, ZString value1Unit, ZDecimal value2, ZString value2Unit, ZString targetUnit)
		{
			var result = 0m;
			var expression = $"{value1Unit}, {value2Unit} > {targetUnit}".Replace("LPA", "ASVX");
			switch (expression)
			{
				case "LTR, ASV > ASVX":
				case "ASV, LTR > ASVX":
					result = value1 * value2 / 100m;
					break;
				case "LTR, ASVX > ASV":
				case "ASV, ASVX > LTR":
					result = value2 / value1 * 100m;
					break;
				case "ASVX, LTR > ASV":
				case "ASVX, ASV > LTR":
					result = value1 / value2 * 100m;
					break;
			}
			return result;
		}

		void SetCustomsQuantity(ZString propertyName, ZDecimal quantity)
		{
			switch (propertyName)
			{
				case JobComInvoiceLine.Schema.JI_CustomsSecondQuantity:
					invoiceLine.JI_CustomsSecondQuantity = quantity;
					break;
				case JobComInvoiceLine.Schema.JI_CustomsThirdQuantity:
					invoiceLine.JI_CustomsThirdQuantity = quantity;
					break;
				case JobComInvoiceLine.Schema.JI_CustomsFourthQuantity:
					invoiceLine.JI_CustomsFourthQuantity = quantity;
					break;
				case JobComInvoiceLine.Schema.JI_CustomsFifthQuantity:
					invoiceLine.JI_CustomsFifthQuantity = quantity;
					break;
			}
		}

		#endregion
	}
}
