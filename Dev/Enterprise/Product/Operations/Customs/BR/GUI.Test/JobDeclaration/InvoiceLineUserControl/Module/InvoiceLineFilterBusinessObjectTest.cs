using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(InvoiceLineFilterBusinessObject))]
	class InvoiceLineFilterBusinessObjectTest : Customs.GUI.Testing.InvoiceLineFilterBusinessObjectTest
	{
		public void TestTextFilterCpc()
		{
			AssertTextFilter(InvoiceLineFilterConstants.Cpc, (line) => line.JI_ProcedureInfo, BRJobMessageTypeList.Codes.Export);
		}

		public void TestTextFilterNfeKey()
		{
			AssertTextFilter(InvoiceLineFilterConstants.NfeKey, (line) => line.JI_NFeNumberInfo, BRJobMessageTypeList.Codes.Export);
		}

		public void TestFilterNfeItemNum()
		{
			AssertTextFilter(InvoiceLineFilterConstants.NfeItemNum, (line) => line.JI_NFeItemNumberInfo, BRJobMessageTypeList.Codes.Export);
		}

		public void TestTextFilterNALADIHS()
		{
			AssertTextFilter(InvoiceLineFilterConstants.NaladiHs, (line) => line.NaladiHsInfo, new string[] { BRJobMessageTypeList.Codes.ImportSiscomex, BRJobMessageTypeList.Codes.ImportLicense });
		}

		public void TestTextFilterImportLicenseNo()
		{
			AssertTextFilter(InvoiceLineFilterConstants.ImportLicenseNo, (line) => line.ImportLicenseNumberInfo, new string[] { BRJobMessageTypeList.Codes.ImportSiscomex });
		}

		public void TestTextFilterManufacturerCode()
		{
			AssertTextFilter(InvoiceLineFilterConstants.ManufacturerCode, (line) =>
			{
				var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
				line.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
				line.ManufacturerDocAddressPK = manufacturer.MainAddress.PK;
				line.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
				return manufacturer.OH_CodeInfo;
			}, BRJobMessageTypeList.Codes.ImportLicense, BRJobMessageTypeList.Codes.ImportSiscomex);
		}

		public void TestTextFilterDutyTaxRegime()
		{
			AssertTextFilter(InvoiceLineFilterConstants.DutyTaxRegime, (line) => line.DutyTaxRegimeInfo, new string[] { BRJobMessageTypeList.Codes.ImportSiscomex, BRJobMessageTypeList.Codes.ImportLicense });
		}

		public void TestTextFilterDutyLegalBase()
		{
			AssertTextFilter(InvoiceLineFilterConstants.DutyLegalBase, (line) => line.DutyLegalBaseInfo, new string[] { BRJobMessageTypeList.Codes.ImportSiscomex, BRJobMessageTypeList.Codes.ImportLicense });
		}

		public void TestNumberRangeFilterNetWeight()
		{
			AssertNumberRangeFilter(InvoiceLineFilterConstants.NetWeight, (line) => line.JI_NetWeightInfo, new[] { BRJobMessageTypeList.Codes.Export, BRJobMessageTypeList.Codes.ImportLicense, BRJobMessageTypeList.Codes.Import, BRJobMessageTypeList.Codes.ImportSiscomex });
		}

		public void TestTextFilterInvoiceUQ()
		{
			AssertTextFilter(InvoiceLineFilterConstants.InvoiceUQ, (line) => line.JI_InvoiceUQInfo, new string[] { BRJobMessageTypeList.Codes.ImportSiscomex, BRJobMessageTypeList.Codes.ImportLicense });
		}

		public void TestTextFilterDrawbackModality()
		{
			AssertTextFilter(InvoiceLineFilterConstants.DrawbackModality, (line) => line.DrawbackModalityInfo, new string[] { BRJobMessageTypeList.Codes.ImportLicense });
		}

		public void TestTextFilterDrawbackCANumber()
		{
			AssertTextFilter(InvoiceLineFilterConstants.DrawbackCANumber, (line) => line.DrawbackCANumberInfo, new string[] { BRJobMessageTypeList.Codes.ImportLicense });
		}

		public void TestTextFilterTariffDetach()
		{
			AssertTextsFilter(InvoiceLineFilterConstants.TariffDetach, (line) => line.TariffDetachs.AddNew().CY_CodeInfo, new string[] { BRJobMessageTypeList.Codes.ImportLicense });
		}

		public void TestFilterContainerNumber()
		{
			AssertNonApplicableMessages(Customs.GUI.InvoiceLineFilterConstants.ContainerNumber, new[] { BRJobMessageTypeList.Codes.Export, BRJobMessageTypeList.Codes.Import, BRJobMessageTypeList.Codes.ImportSiscomex });
		}

		public void TestTextFilterAgreement()
		{
			AssertTextFilter(InvoiceLineFilterConstants.Agreement, (line) => line.JI_SecondaryPreferenceInfo, new string[] { BRJobMessageTypeList.Codes.ImportLicense });
		}

		public void TestTextFilterManufacturerVersion()
		{
			AssertTextFilter(InvoiceLineFilterConstants.ManufacturerVersion, (line) => line.JI_ManufacturerAuthorityVersionInfo, new string[] { BRJobMessageTypeList.Codes.Import });
		}

		public void TestTextFilterCatalogAuthorityVersion()
		{
			AssertTextFilter(InvoiceLineFilterConstants.AuthorityVersion, (line) => line.JI_CatalogAuthorityVersionInfo, new string[] { BRJobMessageTypeList.Codes.Import });
		}

		public void TestTextFilterCatalogAuthorityIdentifier()
		{
			AssertTextFilter(InvoiceLineFilterConstants.AuthorityIdentifier, (line) => line.JI_CatalogAuthorityIdentifierInfo, new string[] { BRJobMessageTypeList.Codes.Import });
		}

		public void TestTextFilterPermitNumber()
		{
			AssertTextsFilter(InvoiceLineFilterConstants.PermitNumber, (line) => line.Permits.AddNew().CSI_ReferenceNumberInfo, new string[] { BRJobMessageTypeList.Codes.Import });
		}

		public void TestTextFilterComplement()
		{
			AssertTextFilter(InvoiceLineFilterConstants.ComplementaryDescription, (line) => line.ComplementaryDescriptionInfo, new string[] { BRJobMessageTypeList.Codes.Import });
		}

		public void TestTextFilterGoodsApplication()
		{
			AssertTextFilter(InvoiceLineFilterConstants.GoodsApplication, (line) => line.JI_GoodsApplicationInfo, new string[] { BRJobMessageTypeList.Codes.Import });
		}

		public void TestGoodsCondition()
		{
			AssertTextFilter(InvoiceLineFilterConstants.GoodsCondition, (line) => line.JI_GoodsConditionInfo, new string[] { BRJobMessageTypeList.Codes.Import });
		}

		public void TestTextFilterManufacturerIndicator()
		{
			AssertTextFilter(InvoiceLineFilterConstants.ManufacturerIndicator, (line) => line.JI_ManufacturerIndicatorInfo, new string[] { BRJobMessageTypeList.Codes.Import, BRJobMessageTypeList.Codes.ImportSiscomex, BRJobMessageTypeList.Codes.ImportLicense });
		}

		public void TestTextFilterGoodsCatalog()
		{
			AssertTextFilter(InvoiceLineFilterConstants.GoodsCatalog, (line) =>
			{
				var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
				line.JI_CGC_Catalog = catalog.PK;
				return catalog.CGC_CatalogCodeInfo;
			}, BRJobMessageTypeList.Codes.Import);
		}

		string GetAssertionMessage(ModuleTextFilter filter) => $"{filter.Description} {filter.SqlComparisonOperator} '{filter.Property}'";

		void AssertFilteredInvoiceLines(ModuleTextFilter filter, SQLComparisonOperator comparison, ZString property, IEnumerable<JobComInvoiceLine> expectedInvoiceLines)
		{
			filter.SqlComparisonOperator = comparison;
			if (!property.IsEmpty)
			{
				filter.Property = ZString.Empty;
				FilterBO.Search();
				AssertContainsExactElementsInAnyOrder(GetAssertionMessage(filter), Declaration.InvoiceLines, Declaration.FilteredInvoiceLines);
			}

			filter.Property = property;
			FilterBO.Search();
			AssertContainsExactElementsInAnyOrder(GetAssertionMessage(filter), expectedInvoiceLines, Declaration.FilteredInvoiceLines);
		}

		void SetPropertyValue(ZPropertyInfo propertyInfo, string value) => GetZPropertyInfo(propertyInfo).SetValueFromString(value);

		ZPropertyInfo GetZPropertyInfo(ZPropertyInfo propertyInfo) => propertyInfo is ZWrappedPropertyInfo wrappedPropertyInfo ? wrappedPropertyInfo.InnerInfo : propertyInfo;

		void AssertTextFilter(string filterName, Func<JobComInvoiceLine, ZPropertyInfo> getPropertyInfo, params string[] applicableMessageTypes)
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();

			var propertyInfo1 = getPropertyInfo.Invoke(invoiceLine1);
			var isStringProperty = GetZPropertyInfo(propertyInfo1) is ZPropertyInfoString;
			var maxLength = isStringProperty ? propertyInfo1.MaxLength : 2;

			var firstValue = "1" + new string('8', maxLength - 1);
			SetPropertyValue(propertyInfo1, firstValue);
			SetPropertyValue(getPropertyInfo.Invoke(invoiceLine2), "1" + new string('9', maxLength - 1));
			SetPropertyValue(getPropertyInfo.Invoke(invoiceLine3), "2" + new string('8', maxLength - 1));
			SetPropertyValue(getPropertyInfo.Invoke(invoiceLine4), "2" + new string('9', maxLength - 1));

			if (propertyInfo1.BizObj == invoiceLine1)
			{
				SetPropertyValue(getPropertyInfo.Invoke(invoiceLine5), ZString.Empty);
			}

			foreach (var messageType in applicableMessageTypes)
			{
				Declaration.JE_MessageType = messageType;

				var filter = (ModuleTextFilter)FilterBO[filterName];
				filter.IsActive = true;

				CombineAssertions(messageType, () =>
				{
					AssertFilteredInvoiceLines(filter, SQLComparisonOperator.Equal, ZString.Empty, new[] { invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4, invoiceLine5 });

					if (isStringProperty)
					{
						AssertFilteredInvoiceLines(filter, SpecialComparisonOperator.IsBlank, "", new[] { invoiceLine5 });
						AssertFilteredInvoiceLines(filter, SpecialComparisonOperator.IsNotBlank, "", new[] { invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4 });
					}
					else
					{
						AssertFilteredInvoiceLines(filter, SpecialComparisonOperator.IsBlank, "", Array.Empty<JobComInvoiceLine>());
						AssertFilteredInvoiceLines(filter, SpecialComparisonOperator.IsNotBlank, "", new[] { invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4, invoiceLine5 });
					}

					if (maxLength > 1)
					{
						AssertFilteredInvoiceLines(filter, SQLComparisonOperator.Equal, firstValue, new[] { invoiceLine1 });
						AssertFilteredInvoiceLines(filter, SQLComparisonOperator.StartsWith, "1", new[] { invoiceLine1, invoiceLine2 });
						AssertFilteredInvoiceLines(filter, SQLComparisonOperator.Contains, "8", new[] { invoiceLine1, invoiceLine3 });
						AssertFilteredInvoiceLines(filter, SQLComparisonOperator.NotEqual, firstValue, new[] { invoiceLine2, invoiceLine3, invoiceLine4, invoiceLine5 });
						AssertFilteredInvoiceLines(filter, SQLComparisonOperator.DoesNotStartWith, "1", new[] { invoiceLine3, invoiceLine4, invoiceLine5 });
						AssertFilteredInvoiceLines(filter, SQLComparisonOperator.NotContains, "8", new[] { invoiceLine2, invoiceLine4, invoiceLine5 });
					}
					else
					{
						AssertFilteredInvoiceLines(filter, SQLComparisonOperator.Equal, "1", new[] { invoiceLine1, invoiceLine2 });
						AssertFilteredInvoiceLines(filter, SQLComparisonOperator.StartsWith, "1", new[] { invoiceLine1, invoiceLine2 });
						AssertFilteredInvoiceLines(filter, SQLComparisonOperator.Contains, "1", new[] { invoiceLine1, invoiceLine2 });
						AssertFilteredInvoiceLines(filter, SQLComparisonOperator.NotEqual, "1", new[] { invoiceLine3, invoiceLine4, invoiceLine5 });
						AssertFilteredInvoiceLines(filter, SQLComparisonOperator.DoesNotStartWith, "1", new[] { invoiceLine3, invoiceLine4, invoiceLine5 });
						AssertFilteredInvoiceLines(filter, SQLComparisonOperator.NotContains, "1", new[] { invoiceLine3, invoiceLine4, invoiceLine5 });
					}

					filterBO = null;
				});

				AssertNonApplicableMessages(filterName, applicableMessageTypes);
			}
		}

		void AssertTextsFilter(string filterName, Func<JobComInvoiceLine, ZPropertyInfo> getPropertyInfo, params string[] applicableMessageTypes)
		{
			Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();

			SetPropertyValue(getPropertyInfo.Invoke(invoiceLine1), "123");
			SetPropertyValue(getPropertyInfo.Invoke(invoiceLine1), "456");
			SetPropertyValue(getPropertyInfo.Invoke(invoiceLine1), "789");
			SetPropertyValue(getPropertyInfo.Invoke(invoiceLine2), "555");
			SetPropertyValue(getPropertyInfo.Invoke(invoiceLine3), "");

			foreach (var messageType in applicableMessageTypes)
			{
				Declaration.JE_MessageType = messageType;

				var filter = (ModuleTextFilter)FilterBO[filterName];
				filter.IsActive = true;

				CombineAssertions(messageType, () =>
				{
					AssertFilteredInvoiceLines(filter, SQLComparisonOperator.Equal, "123", new[] { invoiceLine1 });
					AssertFilteredInvoiceLines(filter, SQLComparisonOperator.StartsWith, "5", new[] { invoiceLine2 });
					AssertFilteredInvoiceLines(filter, SQLComparisonOperator.Contains, "89", new[] { invoiceLine1 });
					AssertFilteredInvoiceLines(filter, SQLComparisonOperator.NotEqual, "555", new[] { invoiceLine1, invoiceLine3 });
					AssertFilteredInvoiceLines(filter, SQLComparisonOperator.DoesNotStartWith, "5", new[] { invoiceLine1, invoiceLine3 });
					AssertFilteredInvoiceLines(filter, SQLComparisonOperator.NotContains, "55", new[] { invoiceLine1, invoiceLine3 });
					AssertFilteredInvoiceLines(filter, SpecialComparisonOperator.IsBlank, "", new[] { invoiceLine3 });
					AssertFilteredInvoiceLines(filter, SpecialComparisonOperator.IsNotBlank, "", new[] { invoiceLine1, invoiceLine2 });
				});

				filterBO = null;
			}
			AssertNonApplicableMessages(filterName, applicableMessageTypes);
		}

		string GetAssertionMessage(ModuleNumberRangeFilter filter) => $"{filter.Description} between '{filter.Property1}' and '{filter.Property2}'";

		void AssertNumberRangeFilter(string filterName, Func<JobComInvoiceLine, ZPropertyInfo> getPropertyInfo, params string[] applicableMessageTypes)
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			getPropertyInfo.Invoke(invoiceLine1).Value = new ZDecimal(0);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			getPropertyInfo.Invoke(invoiceLine2).Value = new ZDecimal(1);
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			getPropertyInfo.Invoke(invoiceLine3).Value = new ZDecimal(2);
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			getPropertyInfo.Invoke(invoiceLine4).Value = new ZDecimal(3);

			foreach (var messageType in applicableMessageTypes)
			{
				Declaration.JE_MessageType = messageType;

				var filter = (ModuleNumberRangeFilter)FilterBO[filterName];
				filter.IsActive = true;

				CombineAssertions(messageType, () =>
				{
					FilterBO.Search();
					AssertContainsExactElementsInAnyOrder(GetAssertionMessage(filter), Declaration.InvoiceLines, Declaration.FilteredInvoiceLines);

					filter.Property1 = 1m;
					filter.Property2 = 2m;
					FilterBO.Search();
					AssertContainsExactElementsInAnyOrder(GetAssertionMessage(filter), new[] { invoiceLine2, invoiceLine3 }, Declaration.FilteredInvoiceLines);

					filterBO = null;
				});

				AssertNonApplicableMessages(filterName, applicableMessageTypes);
			}
		}

		void AssertNonApplicableMessages(string filterName, params string[] applicableMessageTypes)
		{
			var allMessageTypesForTesting = new[] { BRJobMessageTypeList.Codes.Export, BRJobMessageTypeList.Codes.Import, BRJobMessageTypeList.Codes.ImportLicense, BRJobMessageTypeList.Codes.ImportSiscomex };

			foreach (var messageType in allMessageTypesForTesting.Except(applicableMessageTypes))
			{
				Declaration.JE_MessageType = messageType;
				AssertNull($"{filterName} must NOT be available when JE_MessageType is {messageType}", FilterBO[filterName]);

				filterBO = null;
			}
		}

		protected override Customs.GUI.InvoiceLineFilterBusinessObject CreateNewInvoiceLineFilterBusinessObject(Func<IInvoicesProvider> getInvoicesProvider, Func<ZString, ZBool> isColumnAvailable)
		{
			return new InvoiceLineFilterBusinessObject(getInvoicesProvider, isColumnAvailable);
		}

		protected JobDeclaration Declaration => (JobDeclaration)declaration;

		protected Customs.GUI.InvoiceLineFilterBusinessObject FilterBO => filterBO ??= GetNewFilterStripBusinessObject() as InvoiceLineFilterBusinessObject;
	}
}
