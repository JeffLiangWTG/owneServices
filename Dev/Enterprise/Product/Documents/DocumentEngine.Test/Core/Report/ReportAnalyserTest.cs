using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.DocumentEngine.Testing.DocumentPackTest;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ReportAnalyserTest : TempFileTestCase
	{
		public void TestUpdateLookupFilterLinkToScheduledReportRecipientForOrganisation()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Jerry Test", string.Empty, GetTemplateContent(true));
			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Jerry Simple Test", template, Factory);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var command = newFactory.Load<ReportCommand>(reportCommand.PK);
			using (var documentPack = new DocumentPack(command))
			{
				var template2 = DocumentEngineTestHelper.CreateExcelTemplateFromString("Jerry Test 2", string.Empty, GetTemplateContent(false));
				using (var deserializedReport = new Report(new DocumentPack(), template2, Guid.Empty, Core.Constants.DataContext.UnitTest))
				{
					deserializedReport.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
					deserializedReport.PrepareForRender();

					var organisationLookupField = deserializedReport.FilterCollection.OfType<LookupField>().First();
					AssertEquals("LinkToScheduledReportRecipientForOrganisation should be false", false, organisationLookupField.LinkToScheduledReportRecipientForOrganisation);

					documentPack.DeserializeDocPackFromReportCollection(deserializedReport, new NotificationBuffer());
					var report = documentPack[0] as Report;
					report.PrepareForRender();

					AssertEquals("LinkToScheduledReportRecipientForOrganisation should be updated to true", true, organisationLookupField.LinkToScheduledReportRecipientForOrganisation);
				}
			}

			Dictionary<string, string> GetTemplateContent(bool linkToScheduledReportRecipientForOrganisation)
			{
				var templateContent = new Dictionary<string, string>();
				templateContent.Add("Sheet1",
		@"{A}-[#Config]
{A}-[Name=Jerry Test]
{A}-[EmailSubject=<Organisation PK>]
{A}-[Data:ReportData=SELECT OH_FullName FROM dbo.OrgHeader WHERE OH_PK = <Organisation PK>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.OH_FullName>]
{A}-[#EndOfReport]");

				if (linkToScheduledReportRecipientForOrganisation)
				{
					templateContent.Add("Filter",
		@"{A}-[Organisation PK] {B}-[Type] {C}-[Organisation Lookup]
{B}-[LinkToScheduledReportRecipientForOrganisation]
{A}-[#End]");
				}
				else
				{
					templateContent.Add("Filter",
		@"{A}-[Organisation PK] {B}-[Type] {C}-[Organisation Lookup]
{A}-[#End]");
				}

				return templateContent;
			}
		}

		public void TestCountryStandardAddressPositionDefaults()
		{
			var templateContents =
@"{A}-[#config]
{A}-[#BodySection]
{A}-[#LeftHandAddress]
{B}-[LEFT RECIPIENT]
{B}-[LEFT ADDRESS]
{A}-[#RightHandAddress]
{B}-[RIGHT RECIPIENT]
{B}-[RIGHT ADDRESS]
{A}-[#EndAddress]
{B}-[Something in the middle.]
{A}-[#LeftHandAddress]
{B}-[LEFT RECIPIENT 2]
{B}-[LEFT ADDRESS 2]
{A}-[#RightHandAddress]
{B}-[RIGHT RECIPIENT 2]
{B}-[RIGHT ADDRESS 2]
{A}-[#EndAddress]
{A}-[#EndOfReport]";

			var dummy = Factory.New<DummyBusinessObject>();
			var docDataProvider = BODocDataProvider.Get(dummy);

			using (var countryCode = new TemporaryValueSetter<string>(
				value => GlbCompany.CurrentCompany.SetCountry(value),
				GlbCompany.CurrentCompany.Country.Code))
			{
				using (var documentPack = new DocumentPack())
				using (var stream = new MemoryStream())
				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, docDataProvider, stream, templateContents))
				{
					countryCode.Set(Enterprise.Core.Constants.CountryCodes.Australia);
					report.PrepareForRender();
					AssertMultilineASCIIEquals("Left hand side address should of been selected.",
@"{A}-[#config]
{A}-[#BodySection]
{B}-[LEFT RECIPIENT]
{B}-[LEFT ADDRESS]
{B}-[Something in the middle.]
{B}-[LEFT RECIPIENT 2]
{B}-[LEFT ADDRESS 2]
{A}-[#EndOfReport]",
						report.WorkSheetCurrentlyBeingProcessed.ToString());
				}

				using (var documentPack = new DocumentPack())
				using (var stream = new MemoryStream())
				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, docDataProvider, stream, templateContents))
				{
					countryCode.Set(Enterprise.Core.Constants.CountryCodes.Italy);
					report.PrepareForRender();
					AssertMultilineASCIIEquals("Right hand side address should of been selected.",
@"{A}-[#config]
{A}-[#BodySection]
{B}-[RIGHT RECIPIENT]
{B}-[RIGHT ADDRESS]
{B}-[Something in the middle.]
{B}-[RIGHT RECIPIENT 2]
{B}-[RIGHT ADDRESS 2]
{A}-[#EndOfReport]",
						report.WorkSheetCurrentlyBeingProcessed.ToString());
				}

				using (var documentPack = new DocumentPack())
				using (var stream = new MemoryStream())
				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, docDataProvider, stream, templateContents))
				{
					countryCode.Set(Enterprise.Core.Constants.CountryCodes.NewZealand);
					report.PrepareForRender();
					AssertMultilineASCIIEquals("Left hand side address should of been selected.",
@"{A}-[#config]
{A}-[#BodySection]
{B}-[LEFT RECIPIENT]
{B}-[LEFT ADDRESS]
{B}-[Something in the middle.]
{B}-[LEFT RECIPIENT 2]
{B}-[LEFT ADDRESS 2]
{A}-[#EndOfReport]",
						report.WorkSheetCurrentlyBeingProcessed.ToString());
				}
			}
		}

		public void TestLeftAndRightHandSideAddresses()
		{
			var templateContents =
@"{A}-[#config]
{A}-[#BodySection]
{A}-[#LeftHandAddress]
{B}-[LEFT RECIPIENT]
{B}-[LEFT ADDRESS]
{A}-[#RightHandAddress]
{B}-[RIGHT RECIPIENT]
{B}-[RIGHT ADDRESS]
{A}-[#EndAddress]
{B}-[Something in the middle.]
{A}-[#LeftHandAddress]
{B}-[LEFT RECIPIENT 2]
{B}-[LEFT ADDRESS 2]
{A}-[#RightHandAddress]
{B}-[RIGHT RECIPIENT 2]
{B}-[RIGHT ADDRESS 2]
{A}-[#EndAddress]
{A}-[#EndOfReport]";

			var dummy = Factory.New<DummyBusinessObject>();
			var docDataProvider = BODocDataProvider.Get(dummy);

			using (new TemporaryValueSetter<string>(
				value => GlbCompany.CurrentCompany.SetCountry(value),
				GlbCompany.CurrentCompany.Country.Code,
				Enterprise.Core.Constants.CountryCodes.Australia))
			using (var addressPosition = new TemporaryValueSetter<string>(
				value => DocumentsDataRegistry.Instance.AddressPosition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value),
				DocumentsDataRegistry.Instance.AddressPosition.Value))
			{
				using (var documentPack = new DocumentPack())
				using (var stream = new MemoryStream())
				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, docDataProvider, stream, templateContents))
				{
					report.PrepareForRender();
					AssertMultilineASCIIEquals("Left hand side address should of been selected.",
@"{A}-[#config]
{A}-[#BodySection]
{B}-[LEFT RECIPIENT]
{B}-[LEFT ADDRESS]
{B}-[Something in the middle.]
{B}-[LEFT RECIPIENT 2]
{B}-[LEFT ADDRESS 2]
{A}-[#EndOfReport]",
						report.WorkSheetCurrentlyBeingProcessed.ToString());
				}

				using (var documentPack = new DocumentPack())
				using (var stream = new MemoryStream())
				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, docDataProvider, stream, templateContents))
				{
					addressPosition.Set(AddressPositionList.Codes.Right);
					report.PrepareForRender();
					AssertMultilineASCIIEquals("Right hand side address should of been selected.",
@"{A}-[#config]
{A}-[#BodySection]
{B}-[RIGHT RECIPIENT]
{B}-[RIGHT ADDRESS]
{B}-[Something in the middle.]
{B}-[RIGHT RECIPIENT 2]
{B}-[RIGHT ADDRESS 2]
{A}-[#EndOfReport]",
						report.WorkSheetCurrentlyBeingProcessed.ToString());
				}

				using (var documentPack = new DocumentPack())
				using (var stream = new MemoryStream())
				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, docDataProvider, stream, templateContents))
				{
					addressPosition.Set(AddressPositionList.Codes.Left);
					report.PrepareForRender();
					AssertMultilineASCIIEquals("Left hand side address should of been selected.",
@"{A}-[#config]
{A}-[#BodySection]
{B}-[LEFT RECIPIENT]
{B}-[LEFT ADDRESS]
{B}-[Something in the middle.]
{B}-[LEFT RECIPIENT 2]
{B}-[LEFT ADDRESS 2]
{A}-[#EndOfReport]",
						report.WorkSheetCurrentlyBeingProcessed.ToString());
				}
			}
		}

		public void TestInvalidAddressAreaHasLeftHandAddressButMissingRightHandSideAddressAddsError()
		{
			var templateContents =
@"{A}-[#config]
{A}-[#BodySection]
{A}-[#LeftHandAddress]
{B}-[LEFT ADDRESS]
{B}-[RIGHT ADDRESS]
{A}-[#EndAddress]
{A}-[#EndOfReport]";

			var dummy = Factory.New<DummyBusinessObject>();
			var docDataProvider = BODocDataProvider.Get(dummy);

			using (new TemporaryValueSetter<string>(
				value => GlbCompany.CurrentCompany.SetCountry(value),
				GlbCompany.CurrentCompany.Country.Code,
				Enterprise.Core.Constants.CountryCodes.Australia))
			using (var addressPosition = new TemporaryValueSetter<string>(
				value => DocumentsDataRegistry.Instance.AddressPosition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value),
				DocumentsDataRegistry.Instance.AddressPosition.Value))
			{
				using (var documentPack = new DocumentPack())
				using (var stream = new MemoryStream())
				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, docDataProvider, stream, templateContents))
				{
					report.PrepareForRender();
					AssertMultilineASCIIEquals("report.ErrorManager.ToString()",
@"Severity: [Error] Message: [#LeftHandAddress in row number 2 does not have #RightHandAddress.] Cell: [A3] Sheetname: [Document]",
						report.ErrorManager.ToString());
				}
			}
		}

		public void TestInvalidAddressAreaHasLeftHandAddressButMissingEndAddressAddsError()
		{
			var templateContents =
@"{A}-[#config]
{A}-[#BodySection]
{A}-[#LeftHandAddress]
{B}-[LEFT ADDRESS]
{A}-[#RightHandAddress]
{B}-[RIGHT ADDRESS]
{A}-[#EndOfReport]";

			var dummy = Factory.New<DummyBusinessObject>();
			var docDataProvider = BODocDataProvider.Get(dummy);

			using (new TemporaryValueSetter<string>(
				value => GlbCompany.CurrentCompany.SetCountry(value),
				GlbCompany.CurrentCompany.Country.Code,
				Enterprise.Core.Constants.CountryCodes.Australia))
			using (var addressPosition = new TemporaryValueSetter<string>(
				value => DocumentsDataRegistry.Instance.AddressPosition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value),
				DocumentsDataRegistry.Instance.AddressPosition.Value))
			{
				using (var documentPack = new DocumentPack())
				using (var stream = new MemoryStream())
				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, docDataProvider, stream, templateContents))
				{
					report.PrepareForRender();
					AssertMultilineASCIIEquals("report.ErrorManager.ToString()",
@"Severity: [Error] Message: [#LeftHandAddress in row number 2 does not have #EndAddress.] Cell: [A3] Sheetname: [Document]",
						report.ErrorManager.ToString());
				}
			}
		}

		public void TestInvalidAddressAreaHasRightHandAddressButMissingLeftHandAddressAddsError()
		{
			var templateContents =
@"{A}-[#config]
{A}-[#BodySection]
{B}-[LEFT ADDRESS]
{A}-[#RightHandAddress]
{B}-[RIGHT ADDRESS]
{A}-[#EndAddress]
{A}-[#EndOfReport]";

			var dummy = Factory.New<DummyBusinessObject>();
			var docDataProvider = BODocDataProvider.Get(dummy);

			using (new TemporaryValueSetter<string>(
				value => GlbCompany.CurrentCompany.SetCountry(value),
				GlbCompany.CurrentCompany.Country.Code,
				Enterprise.Core.Constants.CountryCodes.Australia))
			using (var addressPosition = new TemporaryValueSetter<string>(
				value => DocumentsDataRegistry.Instance.AddressPosition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value),
				DocumentsDataRegistry.Instance.AddressPosition.Value))
			{
				using (var documentPack = new DocumentPack())
				using (var stream = new MemoryStream())
				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, docDataProvider, stream, templateContents))
				{
					report.PrepareForRender();
					AssertMultilineASCIIEquals("report.ErrorManager.ToString()",
@"Severity: [Error] Message: [#RightHandAddress in row number 3 does not have #LeftHandAddress.] Cell: [A4] Sheetname: [Document]",
						report.ErrorManager.ToString());
				}
			}
		}

		public void TestInvalidAddressAreaHasEndAddressButMissingLeftHandAddressAddsError()
		{
			var templateContents =
@"{A}-[#config]
{A}-[#BodySection]
{B}-[LEFT ADDRESS]
{B}-[RIGHT ADDRESS]
{A}-[#EndAddress]
{A}-[#EndOfReport]";

			var dummy = Factory.New<DummyBusinessObject>();
			var docDataProvider = BODocDataProvider.Get(dummy);

			using (new TemporaryValueSetter<string>(
				value => GlbCompany.CurrentCompany.SetCountry(value),
				GlbCompany.CurrentCompany.Country.Code,
				Enterprise.Core.Constants.CountryCodes.Australia))
			using (var addressPosition = new TemporaryValueSetter<string>(
				value => DocumentsDataRegistry.Instance.AddressPosition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value),
				DocumentsDataRegistry.Instance.AddressPosition.Value))
			{
				using (var documentPack = new DocumentPack())
				using (var stream = new MemoryStream())
				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, docDataProvider, stream, templateContents))
				{
					report.PrepareForRender();
					AssertMultilineASCIIEquals("report.ErrorManager.ToString()",
@"Severity: [Error] Message: [#EndAddress in row number 4 does not have #LeftHandAddress.] Cell: [A5] Sheetname: [Document]",
						report.ErrorManager.ToString());
				}
			}
		}

		public void TestInvalidConditionalsAddsError()
		{
			using (var documentPack = new DocumentPack())
			using (var stream = new MemoryStream())
			{
				var dummy = Factory.New<DummyBusinessObject>();

				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, BODocDataProvider.Get(dummy), stream,
@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#if ""TRUE"" == ""TRUE""]
{B}-[Condition is True.]
{A}-[#else]
{B}-[Condition is False.]
{A}-[#endif]
{A}-[#endif]
{A}-[#if ""FALSE"" =&= ""FALSE""]
{B}-[Condition is True.]
{A}-[#else]
{B}-[Condition is False.]
{A}-[#endif]
{A}-[#else]
{A}-[#if ""TRUE"" == ""FALSE""]
{B}-[Condition is True.]
{A}-[#else]
{B}-[Condition is False.]
{A}-[#endif]
{A}-[#else]
{A}-[#EndOfReport]"))
				{
					report.PrepareForRender();

					CombineAssertions(() =>
					{
						AssertMultilineASCIIEquals("report.PrepareForRender()",
							@"{A}-[#config]
{A}-[#DocumentHeader]
{B}-[Condition is True.]
{A}-[#endif]
{B}-[Condition is False.]
{A}-[#else]
{B}-[Condition is False.]
{A}-[#else]
{A}-[#EndOfReport]",
							report.WorkSheetCurrentlyBeingProcessed.ToString());

						AssertMultilineASCIIEquals("report.ErrorManager.ToString()",
							@"Severity: [Error] Message: [#ELSE in row number 13 does not have #IF] Cell: [A14] Sheetname: [Document]
Severity: [Error] Message: [#ELSE in row number 19 does not have #IF] Cell: [A20] Sheetname: [Document]
Severity: [Error] Message: [#ENDIF in row number 7 does not have #IF] Cell: [A8] Sheetname: [Document]
Severity: [Error] Message: [Result of  ""FALSE"" =&= ""FALSE"" is not a True/False expression] Cell: [A9] Sheetname: [Document]",
							report.ErrorManager.ToString());
					});
				}
			}
		}

		public void TestInvalidEndIfInReportAddsError()
		{
			using (var documentPack = new DocumentPack())
			using (var stream = new MemoryStream())
			{
				var dummy = Factory.New<DummyBusinessObject>();

				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, BODocDataProvider.Get(dummy), stream,
@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#if ""AIR"" == ""AIR""]
{B}-[Condition is True.]
{A}-[#else]
{B}-[Condition is False.]
{A}-[#endif]
{A}-[#endif]
{A}-[#EndOfReport]"))
				{
					report.PrepareForRender();

					CombineAssertions(() =>
					{
						AssertMultilineASCIIEquals("report.PrepareForRender()",
							@"{A}-[#config]
{A}-[#DocumentHeader]
{B}-[Condition is True.]
{A}-[#endif]
{A}-[#EndOfReport]",
							report.WorkSheetCurrentlyBeingProcessed.ToString());

						AssertMultilineASCIIEquals("report.ErrorManager.ToString()",
							@"Severity: [Error] Message: [#ENDIF in row number 7 does not have #IF] Cell: [A8] Sheetname: [Document]",
							report.ErrorManager.ToString());
					});
				}
			}
		}

		public void TestInvalidElseInReportAddsError()
		{
			using (var documentPack = new DocumentPack())
			using (var stream = new MemoryStream())
			{
				var dummy = Factory.New<DummyBusinessObject>();

				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, BODocDataProvider.Get(dummy), stream,
@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#if ""AIR"" == ""AIR""]
{B}-[Condition is True.]
{A}-[#else]
{B}-[Condition is False.]
{A}-[#endif]
{A}-[#else]
{A}-[#EndOfReport]"))
				{
					report.PrepareForRender();

					CombineAssertions(() =>
					{
						AssertMultilineASCIIEquals("report.PrepareForRender()",
							@"{A}-[#config]
{A}-[#DocumentHeader]
{B}-[Condition is True.]
{A}-[#else]
{A}-[#EndOfReport]",
							report.WorkSheetCurrentlyBeingProcessed.ToString());

						AssertMultilineASCIIEquals("report.ErrorManager.ToString()",
							@"Severity: [Error] Message: [#ELSE in row number 7 does not have #IF] Cell: [A8] Sheetname: [Document]",
							report.ErrorManager.ToString());
					});
				}
			}
		}

		public void TestConditionalWithExpressionThatCanNotEvaluateUsesFalseAndAddsError()
		{
			using (var documentPack = new DocumentPack())
			using (var stream = new MemoryStream())
			{
				var dummy = Factory.New<DummyBusinessObject>();

				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, BODocDataProvider.Get(dummy), stream,
@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#if ""AIR"" =&= ""AIR""]
{B}-[Condition is True.]
{A}-[#else]
{B}-[Condition is False.]
{A}-[#endif]
{A}-[#EndOfReport]"))
				{
					report.PrepareForRender();

					CombineAssertions(() =>
					{
						AssertMultilineASCIIEquals("report.PrepareForRender()",
							@"{A}-[#config]
{A}-[#DocumentHeader]
{B}-[Condition is False.]
{A}-[#EndOfReport]",
							report.WorkSheetCurrentlyBeingProcessed.ToString());

						AssertMultilineASCIIEquals("report.ErrorManager.ToString()",
							@"Severity: [Error] Message: [Result of  ""AIR"" =&= ""AIR"" is not a True/False expression] Cell: [A3] Sheetname: [Document]",
							report.ErrorManager.ToString());
					});
				}
			}
		}

		public void TestAnalyzeWithFieldsNotFoundInAConditionalExpressionAddsToErrorListWithCellReference()
		{
			using (var documentPack = new DocumentPack())
			{
				using (var stream = new MemoryStream())
				{
					var dummy = Factory.New<DummyBusinessObject>();

					using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, BODocDataProvider.Get(dummy), stream,
@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#if ""<TransportMode>"" == ""AIR"" || (""<ArrivalConsol.ConsolMode>"" != ""BCN"" && ""<PackingMode>"" != ""FCL"")]
{B}-[Condition is True.]
{A}-[#else]
{B}-[Condition is False.]
{A}-[#endif]
{A}-[#EndOfReport]"))
					{
						report.PrepareForRender();

						CombineAssertions(() =>
						{
							AssertMultilineASCIIEquals("report.PrepareForRender()",
								@"{A}-[#config]
{A}-[#DocumentHeader]
{B}-[Condition is False.]
{A}-[#EndOfReport]", report.WorkSheetCurrentlyBeingProcessed.ToString());

							AssertMultilineASCIIEquals("report.ErrorManager.ToString()",
								@"Severity: [Error] Message: [Field <ArrivalConsol.ConsolMode> not found on DataSource Type [DummyBusinessObject].] Cell: [A3] Sheetname: [Document]",
								report.ErrorManager.ToString());
						});
					}
				}
			}
		}

		public void TestProcessDisableFixedValueCache()
		{
			using (var documentPack = new DocumentPack())
			{
				var dummy = Factory.New<DummyBusinessObject>();

				using (var stream = new MemoryStream())
				{
					using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, BODocDataProvider.Get(dummy), stream,
						@"{A}-[#config]
{A}-[DISABLEFIXEDVALUECACHE=N]
{A}-[#EndOfReport]"))
					{
						report.PrepareForRender();
						Assert(!report.DisableFixedValueCache);
					}
				}

				using (var stream = new MemoryStream())
				{
					using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, BODocDataProvider.Get(dummy), stream,
						@"{A}-[#config]
{A}-[DISABLEFIXEDVALUECACHE=Y]
{A}-[#EndOfReport]"))
					{
						report.PrepareForRender();
						Assert(report.DisableFixedValueCache);
					}
				}

				GlbCompany.CurrentCompany.GC_Code = "EDI";
				using (var stream = new MemoryStream())
				{
					using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, BODocDataProvider.Get(dummy), stream,
						@"{A}-[#config]
{A}-[DISABLEFIXEDVALUECACHE=""<CompanyCode>"" == ""ABC""]
{A}-[#EndOfReport]"))
					{
						report.PrepareForRender();
						Assert(!report.DisableFixedValueCache);
					}
				}

				using (var stream = new MemoryStream())
				{
					using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, BODocDataProvider.Get(dummy), stream,
						@"{A}-[#config]
{A}-[DISABLEFIXEDVALUECACHE=""<CompanyCode>"" == ""EDI""]
{A}-[#EndOfReport]"))
					{
						report.PrepareForRender();
						Assert(report.DisableFixedValueCache);
					}
				}
			}
		}

		public void TestEmptyConfigArea()
		{
			using (var documentPack = new DocumentPack())
			using (var stream = new MemoryStream())
			{
				var dummy = Factory.New<DummyBusinessObject>();
				var testContent =
@"{A}-[#ConfigurableSection:GEN:Invoice, Invoice Letterhead]
{A}-[#ConfigurableSection:GEN:Invoice, Invoice Document Title]
{A}-[#EndOfReport]";

				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(
					documentPack,
					BODocDataProvider.Get(dummy),
					stream,
					testContent
				))
				{
					report.PrepareForRender();
					CombineAssertions(() =>
					{
						AssertMultilineASCIIEquals(
							"report.PrepareForRender()",
							testContent,
							report.WorkSheetCurrentlyBeingProcessed.ToString());

						AssertMultilineASCIIEquals(
							"report.ErrorManager.ToString()",
							$"Severity: [Error] Message: [Template must have a #Config area at the top of the first tab, but it is: \"{testContent}\"] Cell: [A1] Sheetname: [Document]",
							report.ErrorManager.ToString());
					});
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValueProviderShouldNotBeFixedValueProviderWhenDisableFixedValueCache()
		{
			var list = new UserControlProviderList();
			var field = new MultipleChoice(Factory)
			{
				DisplayName = "JohnSnow",
				Value = "123",
				DefaultExpression = "<CompanyCode>"
			};
			list.Add(field);

			var excelTemplate = new ExcelTemplateForUnitTesting("UDF with DisableFixedValueCache.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate, new DocumentWrapperForTesting("Main"), "ss", list, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				Assert(report.DisableFixedValueCache);
				Assert("ValueProvider should not be FixedValueProvider if DisableFixedValueCache is true", !(report.MacroTranslator.GetValueProvider(Passes.FirstPass, "<JohnSnow>") is FixedValueProvider));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideColumnIfWithNonExistantOptionalColumnSuspendsErrorMessage()
		{
			AssertHideColumnIfWithNonExistantOptionalColumnSuspendsErrorMessage(false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideColumnIfWithNonExistantOptionalColumnSuspendsErrorMessage_WithCustomizedTemplate()
		{
			AssertHideColumnIfWithNonExistantOptionalColumnSuspendsErrorMessage(true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSavesToWithLookUpFilter()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SavesToWithLookUpFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				AssertEquals("Precondition - rpt.ErrorManager.HasErrors = false", false, report.ErrorManager.HasErrors);
				report.PrepareForRender();
				var saveToFilterField = report.ColumnHeadingManager.SaveToFilterField;
				AssertEquals("saveToFilterField", "Vessel/Journey", saveToFilterField);
				AssertEquals("saveToFilterField should be a LookupField", true, report.FilterCollection[saveToFilterField] is LookupField);
				AssertEquals("ErrorManager.HasErrors = false", false, report.ErrorManager.HasErrors);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSavesToWithNonLookUpFilter()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SavesToWithNonLookUpFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				AssertEquals("Precondition - rpt.ErrorManager.HasErrors = false", false, report.ErrorManager.HasErrors);
				report.PrepareForRender();
				var saveToFilterField = report.ColumnHeadingManager.SaveToFilterField;
				AssertEquals("saveToFilterField", "Flight/Voyage", saveToFilterField);
				AssertEquals("saveToFilterField should NOT be a LookupField", false, report.FilterCollection[saveToFilterField] is LookupField);
				AssertEquals("ErrorManager.HasErrors = true", true, report.ErrorManager.HasErrors);
				AssertMultilineASCIIEquals("Flight/Voyage", string.Format(@"
Severity: [Warning] Message: [Filter [Flight/Voyage] is not a table based filter field. [SavesTo] must point to a table based filter field. You will not be able to save configurations against this filter value.] Cell: [A4] Sheetname: [Config] TemplatePath: [{0}]

No MenuItem Found on the Report.

No StmTemplate Found on the Report.

Not Running from Scheduled Report.".Trim(), report.Template.TemplateSourceLocation).Trim(), report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}] TemplatePath: [{4}]", true));
			}
		}

		public void TestAnalyserDealsWithHiddenTemplates()
		{
			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);

					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];

					workSheet.SheetNameOverride = "First Sheet";
					workSheet.UpdateSheetName();
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "HideSheetIf=\"Black\"==\"White\"";
					workSheet[4, 0] = "#SectionBody";
					workSheet[5, 1] = "First Worksheet";
					workSheet[6, 0] = "#EndOfReport";

					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = new Report(pack, excelTemplate))
				{
					report.PrepareForRender();
					AssertEquals("Hidden Sheets", "", string.Join(",", report.HiddenSheetsNames.ToArray()));
					AssertNotEquals("report.WorkSheetCurrentlyBeingProcessed", "", report.WorkSheetCurrentlyBeingProcessed.ToString());
				}
			}

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);

					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];

					workSheet.SheetNameOverride = "First Sheet";
					workSheet.UpdateSheetName();
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "HideSheetIf=\"White\"==\"White\"";
					workSheet[4, 0] = "#SectionBody";
					workSheet[5, 1] = "First Worksheet";
					workSheet[6, 0] = "#EndOfReport";

					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = new Report(pack, excelTemplate))
				{
					report.PrepareForRender();
					AssertEquals("Hidden Sheets", "First Sheet", string.Join(",", report.HiddenSheetsNames.ToArray()));
					AssertEquals("report.WorkSheetCurrentlyBeingProcessed", "", report.WorkSheetCurrentlyBeingProcessed.ToString());
				}
			}
		}

		public void TestCustomPageHeight()
		{
			var templateContents =
				@"{A}-[#config]
{A}-[Name=Test]
{A}-[PageStyle=Custom]
{A}-[PageHeight=100]
{A}-[#SectionBody]
{A}-[#EndOfReport]";

			using (var report = new Report(DocumentPack.EmptyPack, DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, templateContents)))
			{
				report.PrepareForRender();
				AssertEquals("PageHeight should be set according to the value of PageHeight parameter", 5700, report.Analyser.PageHeight);
			}

			templateContents =
				@"{A}-[#config]
{A}-[Name=Test]
{A}-[PageStyle=Custom]
{A}-[#SectionBody]
{A}-[#EndOfReport]";
			using (var report = new Report(DocumentPack.EmptyPack, DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, templateContents)))
			{
				report.PrepareForRender();
				AssertEquals("PageHeight should be defaulted if PageStyle=Custom while PageHeight is not set", int.MaxValue, report.Analyser.PageHeight);
			}
		}

		public void TestNoSectionBodyAddsError()
		{
			string templateContents =
@"{A}-[#config]
{A}-[#SectionPageHeader]
{A}-[#EndOfReport]";

			var dummy = Factory.New<DummyBusinessObject>();
			var docDataProvider = BODocDataProvider.Get(dummy);

			using (var documentPack = new DocumentPack())
			using (var stream = new MemoryStream())
			using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, docDataProvider, stream, templateContents))
			{
				string errorMessage = string.Concat("Severity: [Error] Message: [Every Section in a template must contain a #SectionBody.\r\nWorkSheetCurrentlyBeingProcessed:\r\n", templateContents, "] Cell: [A2] Sheetname: [Document]");

				report.PrepareForRender();
				AssertMultilineASCIIEquals("report.ErrorManager.ToString()", errorMessage, report.ErrorManager.ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOutOfOrderSectionsGetPickedUpIfTheUserWantsToSeeThem()
		{
			GlbStaff.CurrentUser.SetWantsToSeeDocumentTemplateErrorsForTesting(true);
			try
			{
				ExcelTemplate excelTemplate = new ExcelTemplateForUnitTesting("MultiSectionTemplateOutOfOrder.xls", TestFilesSubFolder.AreaTestFiles);
				using (var report = new Report(pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
				{
					report.PrepareForRender();
					AssertEquals("Report.Errors", "Severity: [Error] Message: [#FirstPageFooter area must come before a #BackPage area.] Cell: [A39]",
											report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
				}
			}
			finally
			{
				GlbStaff.CurrentUser.SetWantsToSeeDocumentTemplateErrorsForTesting(false);
			}
		}

		public void TestDeliverTwiceDoesNotResetSortOrder()
		{
			var instructions = new DeliveryInstructions();
			var contact1 = instructions.Recipients.AddNew();
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact1.Name = "Zubin";
			contact1.Email = "test@example.com";

			using (DocumentPack pack = new TestableDocumentPack())
			{
				var template = TestReport;
				using (Report report = new Report(pack, template))
				{
					pack.Add(report);

					instructions.Destination = DeliveryInstructionDestination.Preview;
					pack.Run(instructions);

					AssertEquals("1 report in pack", 1, pack.Count);
					Report retrievedReport = (Report)pack[0];

					RuntimeOptions.SortOrder order = new RuntimeOptions.SortOrder("Zubin", "ZUBINCOL");
					report.SortOrderCollection.Add(order);
					SortOrderCollection currentSortOrders = report.SortOrderCollection;

					AssertEquals("Precondition: Sort Order Collection contains my sort", order, currentSortOrders[0]);

					instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
					pack.Run(instructions);

					AssertEquals("1 report still exists", 1, pack.Count);
					retrievedReport = (Report)pack[0];

					Assert("Sort Order Collection contains my filter", report.SortOrderCollection.Count > 0);
					AssertEquals("Sort Order Collection contains my sort", order, report.SortOrderCollection[0]);
				}
			}
		}

		public void TestDeliverTwiceDoesNotResetFilters()
		{
			var instructions = new DeliveryInstructions();
			var contact1 = instructions.Recipients.AddNew();
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact1.Name = "Zubin";
			contact1.Email = "test@example.com";

			using (DocumentPack pack = new TestableDocumentPack())
			{
				var template = TestReport;
				using (Report report = new Report(pack, template))
				{
					pack.Add(report);

					instructions.Destination = DeliveryInstructionDestination.Preview;
					pack.Run(instructions);

					AssertEquals("1 report in pack", 1, pack.Count);
					Report retrievedReport = (Report)pack[0];

					DummyFilter filter = new DummyFilter("DUMMY = 'DUMMY'");
					report.FilterCollection.Add(filter);
					CollectionOfIFilter currentFilters = report.FilterCollection;

					AssertEquals("Precondition: Filter Collection contains my filter", filter, currentFilters[0]);

					instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
					pack.Run(instructions);

					AssertEquals("1 report still exists", 1, pack.Count);
					retrievedReport = (Report)pack[0];

					Assert("Filter Collection contains my filter", report.FilterCollection.Count > 0);
					AssertEquals("Filter Collection contains my filter", filter, report.FilterCollection[0]);
				}
			}
		}

		public void TestReportName()
		{
			using (var rpt = new Report(pack, NewStyleTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(rpt);
				rpt.PrepareForRender();
				AssertEquals("TestTemplate", rpt.Name);
			}
		}

		public void TestReportVersion()
		{
			using (var rpt = new Report(pack, NewStyleTemplate))
			{
				rpt.PrepareForRender();
				var testReportAnalyser = rpt.Analyser;
				AssertEquals(130, testReportAnalyser.Config.ReportVersion);
			}
		}

		public void TestReportDataSources()
		{
			using (var rpt = new Report(pack, NewStyleTemplate))
			{
				rpt.PrepareForRender();
				var testReportAnalyser = rpt.Analyser;
				AssertEquals(4, testReportAnalyser.ReportSQLSources.Count);
				AssertEquals("Notes:select * from ##DocEngineTest", testReportAnalyser.ReportSQLSources[0].TableNameAndSelectStatement);
				AssertEquals("Job:##JobTest", testReportAnalyser.ReportSQLSources[1].TableNameAndSelectStatement);
				AssertEquals("Lines:##LinesTest:NoWhereClause", testReportAnalyser.ReportSQLSources[2].TableNameAndSelectStatement);
				AssertEquals("Header:select * from ##HeaderTest", testReportAnalyser.ReportSQLSources[3].TableNameAndSelectStatement);
			}
		}

		public void TestReportDataSources_DataSourceTypes()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[Data:ReportData1=SELECT 'A' as ColumnOne]
{A}-[EDWData:ReportData2=SELECT 'B' as ColumnTwo]
{A}-[EDWOnlyData:ReportData3=SELECT 'C' as ColumnThree]
{A}-[#EndOfReport]");
			using (var documentPack = new DocumentPack(Factory.New<StmMenuItem>()))
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var testReportAnalyser = report.Analyser;
				AssertEquals(3, testReportAnalyser.ReportSQLSources.Count);
				Assert(testReportAnalyser.ReportSQLSources.Any(x => x.DataSourceType == DataSourceTypes.NormalData && x.TableNameAndSelectStatement == "ReportData1:SELECT 'A' as ColumnOne"));
				Assert(testReportAnalyser.ReportSQLSources.Any(x => x.DataSourceType == DataSourceTypes.EdwData && x.TableNameAndSelectStatement == "ReportData2:SELECT 'B' as ColumnTwo"));
				Assert(testReportAnalyser.ReportSQLSources.Any(x => x.DataSourceType == DataSourceTypes.EdwData && x.TableNameAndSelectStatement == "ReportData3:SELECT 'C' as ColumnThree"));
			}
		}

		public void TestSectionTableNameAndGroupBys()
		{
			using (var rpt = new Report(pack, NewStyleTemplate))
			{
				rpt.PrepareForRender();
				var testReportAnalyser = rpt.Analyser;

				AssertEquals("Lines", testReportAnalyser.Sections[0].TableName);
				AssertEquals(1, ((GroupByArea)testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[1]).GroupByColumns.Length);
				AssertEquals("Lines.AccountingGroupName", ((GroupByArea)testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[1]).GroupByColumns[0]);
			}
		}

		public void TestConfigArea()
		{
			using (var rpt = new Report(pack, NewStyleTemplate))
			{
				rpt.PrepareForRender();
				var testReportAnalyser = rpt.Analyser;
				Assert(testReportAnalyser.Areas[0] is ConfigArea);
				AssertEquals(0, testReportAnalyser.Areas[0].StartingRow);
				AssertEquals(8, testReportAnalyser.Areas[0].End);
			}
		}

		public void TestSections()
		{
			using (var rpt = new Report(pack, NewStyleTemplate))
			{
				rpt.PrepareForRender();
				var testReportAnalyser = rpt.Analyser;

				AssertEquals(1, testReportAnalyser.Sections.Count);

				AssertNotNull(testReportAnalyser.Sections[0].SectionHeader);
				AssertEquals(20, testReportAnalyser.Sections[0].SectionHeader.StartingRow);
				AssertEquals(34, testReportAnalyser.Sections[0].SectionHeader.End);

				AssertNotNull(testReportAnalyser.Sections[0].SectionPageHeader);
				Assert(testReportAnalyser.Sections[0].SectionPageHeader is SectionPageHeaderArea);
				AssertEquals(35, testReportAnalyser.Sections[0].SectionPageHeader.StartingRow);
				AssertEquals(38, testReportAnalyser.Sections[0].SectionPageHeader.End);

				Assert(testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[0] is SectionBodyArea);
				AssertEquals(39, testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[0].StartingRow);
				AssertEquals(40, testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[0].End);

				Assert(testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[1] is GroupByArea);
				AssertEquals(41, testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[1].StartingRow);
				AssertEquals(42, testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[1].End);

				AssertNotNull(testReportAnalyser.Sections[0].SectionPageFooter);
				Assert(testReportAnalyser.Sections[0].SectionPageFooter is SectionPageFooterArea);
				AssertEquals(43, testReportAnalyser.Sections[0].SectionPageFooter.StartingRow);
				AssertEquals(44, testReportAnalyser.Sections[0].SectionPageFooter.End);

				AssertNotNull(testReportAnalyser.Sections[0].SectionFooter);
				AssertEquals(45, testReportAnalyser.Sections[0].SectionFooter.StartingRow);
				AssertEquals(49, testReportAnalyser.Sections[0].SectionFooter.End);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSectionsForBookingConfirmationDoc()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("BookingConfirmationDoc.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate))
			{
				rpt.PrepareForRender();
				ReportAnalyser testReportAnalyser = rpt.Analyser;

				AssertEquals(0, testReportAnalyser.Sections.Count);
				AssertNotNull(testReportAnalyser.DocumentHeader);
				AssertEquals(5, testReportAnalyser.DocumentHeader.StartingRow);
				AssertEquals(32, testReportAnalyser.DocumentHeader.End);
				AssertNull(testReportAnalyser.PageHeader);
				AssertNull(testReportAnalyser.PageFooter);
				AssertNull(testReportAnalyser.DocumentFooter);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSectionsOfBookingSummary()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BookingSummaryDoc.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate))
			{
				rpt.PrepareForRender();
				ReportAnalyser testReportAnalyser = rpt.Analyser;

				AssertNotNull(testReportAnalyser.DocumentHeader);
				Assert(testReportAnalyser.DocumentHeader is DocumentHeaderArea);
				AssertEquals(27, testReportAnalyser.DocumentHeader.StartingRow);
				AssertEquals(39, testReportAnalyser.DocumentHeader.End);

				AssertEquals(2, testReportAnalyser.Sections.Count);

				AssertNull(testReportAnalyser.Sections[0].SectionHeader);

				AssertNotNull(testReportAnalyser.Sections[0].SectionPageHeader);
				Assert(testReportAnalyser.Sections[0].SectionPageHeader is SectionPageHeaderArea);
				AssertEquals(40, testReportAnalyser.Sections[0].SectionPageHeader.StartingRow);
				AssertEquals(41, testReportAnalyser.Sections[0].SectionPageHeader.End);

				Assert(testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[0] is SectionBodyArea);
				AssertEquals(42, testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[0].StartingRow);
				AssertEquals(43, testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[0].End);

				AssertNull(testReportAnalyser.Sections[0].SectionPageFooter);

				AssertNotNull(testReportAnalyser.Sections[0].SectionFooter);
				AssertEquals(44, testReportAnalyser.Sections[0].SectionFooter.StartingRow);
				AssertEquals(46, testReportAnalyser.Sections[0].SectionFooter.End);

				AssertNull(testReportAnalyser.Sections[1].SectionHeader);

				AssertNotNull(testReportAnalyser.Sections[1].SectionPageHeader);
				Assert(testReportAnalyser.Sections[1].SectionPageHeader is SectionPageHeaderArea);
				AssertEquals(47, testReportAnalyser.Sections[1].SectionPageHeader.StartingRow);
				AssertEquals(48, testReportAnalyser.Sections[1].SectionPageHeader.End);

				Assert(testReportAnalyser.Sections[1].SectionBodyAndGroupByAreas[0] is SectionBodyArea);
				AssertEquals(49, testReportAnalyser.Sections[1].SectionBodyAndGroupByAreas[0].StartingRow);
				AssertEquals(50, testReportAnalyser.Sections[1].SectionBodyAndGroupByAreas[0].End);

				AssertNull(testReportAnalyser.Sections[1].SectionPageFooter);

				AssertNotNull(testReportAnalyser.Sections[1].SectionFooter);
				AssertEquals(51, testReportAnalyser.Sections[1].SectionFooter.StartingRow);
				AssertEquals(52, testReportAnalyser.Sections[1].SectionFooter.End);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSectionsOfContainerManifest()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("ContainerManifestDoc.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate))
			{
				rpt.PrepareForRender();
				ReportAnalyser testReportAnalyser = rpt.Analyser;

				AssertEquals(1, testReportAnalyser.Sections.Count);

				AssertNotNull(testReportAnalyser.Sections[0].SectionHeader);
				AssertEquals(9, testReportAnalyser.Sections[0].SectionHeader.StartingRow);
				AssertEquals(14, testReportAnalyser.Sections[0].SectionHeader.End);

				AssertNotNull(testReportAnalyser.Sections[0].SectionPageHeader);
				Assert(testReportAnalyser.Sections[0].SectionPageHeader is SectionPageHeaderArea);
				AssertEquals(15, testReportAnalyser.Sections[0].SectionPageHeader.StartingRow);
				AssertEquals(17, testReportAnalyser.Sections[0].SectionPageHeader.End);

				Assert(testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[0] is SectionBodyArea);
				AssertEquals(18, testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[0].StartingRow);
				AssertEquals(19, testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[0].End);

				AssertNull(testReportAnalyser.Sections[0].SectionPageFooter);

				AssertNotNull(testReportAnalyser.Sections[0].SectionFooter);
				AssertEquals(20, testReportAnalyser.Sections[0].SectionFooter.StartingRow);
				AssertEquals(22, testReportAnalyser.Sections[0].SectionFooter.End);
			}
		}

		public void TestSectionAreass()
		{
			using (var rpt = new Report(pack, NewStyleTemplate))
			{
				rpt.PrepareForRender();
				var testReportAnalyser = rpt.Analyser;

				AssertEquals(1, testReportAnalyser.Sections.Count);

				AssertEquals(2, testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas.Count);

				Assert(testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[0] is SectionBodyArea);
				AssertEquals(39, testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[0].StartingRow);
				AssertEquals(40, testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[0].End);

				Assert(testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[1] is GroupByArea);
				AssertEquals(41, testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[1].StartingRow);
				AssertEquals(42, testReportAnalyser.Sections[0].SectionBodyAndGroupByAreas[1].End);
			}
		}

		[ExpectNoExceptions]
		public void TestNoExceptionsWhenPushingAreasWithIncorrectOrders()
		{
			var templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#PageHeader]
{A}-[#SectionPageHeader]
{A}-[#SectionBody]
{A}-[#PageFooter]
{A}-[#EndOfReport]";

			using (var report = new Report(DocumentPack.EmptyPack, DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, templateContents)))
			{
				report.Renderer.Render();
			}
		}

		public void TestPushAreasIntoTypedPropertiesAndSectionsList()
		{
			var templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#PageHeader]
{A}-[#SectionPageHeader]
{A}-[#SectionBody]
{A}-[#PageFooter]
{A}-[#EndOfReport]";

			using (var report = new Report(DocumentPack.EmptyPack, DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, templateContents)))
			{
				report.PrepareForRender();

				AssertEquals(2, report.Analyser.Sections.Count);
				AssertEquals(1, report.Analyser.Sections[0].SectionBodyAndGroupByAreas.Count);
				AssertEquals(1, report.Analyser.Sections[1].SectionBodyAndGroupByAreas.Count);
				Assert(report.Analyser.Sections[0].SectionBodyAndGroupByAreas[0] is SectionBodyArea);
				Assert(report.Analyser.Sections[1].SectionBodyAndGroupByAreas[0] is SectionBodyArea);
			}

			templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#SectionBody]
{A}-[#GroupBy:Collection.Z0_Number]
{A}-[#SectionFooter]
{A}-[#PageHeader]
{A}-[#SectionBody]
{A}-[#SectionFooter]
{A}-[#EndOfReport]";

			using (var report = new Report(DocumentPack.EmptyPack, DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, templateContents)))
			{
				report.PrepareForRender();

				AssertEquals(3, report.Analyser.Sections.Count);
				AssertEquals(1, report.Analyser.Sections[0].SectionBodyAndGroupByAreas.Count);
				AssertEquals(2, report.Analyser.Sections[1].SectionBodyAndGroupByAreas.Count);
				AssertEquals(1, report.Analyser.Sections[2].SectionBodyAndGroupByAreas.Count);
				Assert(report.Analyser.Sections[0].SectionBodyAndGroupByAreas[0] is SectionBodyArea);
				Assert(report.Analyser.Sections[1].SectionBodyAndGroupByAreas[0] is SectionBodyArea);
				Assert(report.Analyser.Sections[2].SectionBodyAndGroupByAreas[0] is SectionBodyArea);
				Assert(report.Analyser.Sections[1].SectionBodyAndGroupByAreas[1] is GroupByArea);
			}

			templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#PageHeader]
{A}-[#SectionBody]
{A}-[#GroupBy:Collection.Z0_Number]
{A}-[#GroupBy:Collection.Z0_Number]
{A}-[#SectionBody]
{A}-[#GroupBy:Collection.Z0_Number]
{A}-[#GroupBy:Collection.Z0_Number]
{A}-[#GroupBy:Collection.Z0_Number]
{A}-[#SectionFooter]
{A}-[#PageFooter]
{A}-[#EndOfReport]";

			using (var report = new Report(DocumentPack.EmptyPack, DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, templateContents)))
			{
				report.PrepareForRender();

				AssertEquals(3, report.Analyser.Sections.Count);
				AssertEquals(1, report.Analyser.Sections[0].SectionBodyAndGroupByAreas.Count);
				AssertEquals(3, report.Analyser.Sections[1].SectionBodyAndGroupByAreas.Count);
				AssertEquals(4, report.Analyser.Sections[2].SectionBodyAndGroupByAreas.Count);
				Assert(report.Analyser.Sections[0].SectionBodyAndGroupByAreas[0] is SectionBodyArea);
				Assert(report.Analyser.Sections[1].SectionBodyAndGroupByAreas[0] is SectionBodyArea);
				Assert(report.Analyser.Sections[2].SectionBodyAndGroupByAreas[0] is SectionBodyArea);
				Assert(report.Analyser.Sections[1].SectionBodyAndGroupByAreas[report.Analyser.Sections[1].SectionBodyAndGroupByAreas.Count - 1] is GroupByArea);
				Assert(report.Analyser.Sections[2].SectionBodyAndGroupByAreas[report.Analyser.Sections[2].SectionBodyAndGroupByAreas.Count - 1] is GroupByArea);
			}
		}

		public void TestDocHeader()
		{
			using (var rpt = new Report(pack, NewStyleTemplate))
			{
				rpt.PrepareForRender();
				var testReportAnalyser = rpt.Analyser;

				AssertNotNull("TestReportAnalyser.DocumentHeader should not be null", testReportAnalyser.DocumentHeader);
				AssertEquals(9, testReportAnalyser.DocumentHeader.StartingRow);
				AssertEquals(10, testReportAnalyser.DocumentHeader.End);
			}
		}

		public void TestPageHeader()
		{
			using (var rpt = new Report(pack, NewStyleTemplate))
			{
				rpt.PrepareForRender();
				var testReportAnalyser = rpt.Analyser;

				AssertNotNull(testReportAnalyser.PageHeader);
				AssertEquals(11, testReportAnalyser.PageHeader.StartingRow);
				AssertEquals(19, testReportAnalyser.PageHeader.End);
			}
		}

		public void TestPageFooter()
		{
			using (var rpt = new Report(pack, NewStyleTemplate))
			{
				rpt.PrepareForRender();
				var testReportAnalyser = rpt.Analyser;

				AssertNotNull(testReportAnalyser.PageFooter);
				AssertEquals(50, testReportAnalyser.PageFooter.StartingRow);
				AssertEquals(51, testReportAnalyser.PageFooter.End);
			}
		}

		public void TestDocFooter()
		{
			using (var rpt = new Report(pack, NewStyleTemplate))
			{
				rpt.PrepareForRender();
				var testReportAnalyser = rpt.Analyser;

				AssertNotNull(testReportAnalyser.DocumentFooter);
				AssertEquals(52, testReportAnalyser.DocumentFooter.StartingRow);
				AssertEquals(59, testReportAnalyser.DocumentFooter.End);
			}
		}

		public void TestAddDataSource()
		{
			using (var rpt = new Report(pack, TestReport))
			{
				var era = new ExposedReportAnalyser(rpt);
				era.ReportSQLSources.Clear();

				era.ExposedAddDataSource(@"data:foo=select * from bar");
				AssertEquals("Report data source count", 1, era.ReportSQLSources.Count);
				AssertEquals("Data source 1 table name", "foo:select * from bar", era.ReportSQLSources[0].TableNameAndSelectStatement);
				AssertEquals("Data source 1 params", 0, era.DataSourceParameters.Count);
			}
		}

		public void TestAddDataSourceWithParameters()
		{
			var excelTemplate = TestReport;
			using (var report = new Report(pack, excelTemplate))
			{
				var analyzer = new ExposedReportAnalyser(report);
				{
					analyzer.ReportSQLSources.Clear();

					analyzer.ExposedAddDataSource(@"data:foo=bar(<Param 1 name>, <Param 2 name>)");
					AssertEquals("Report data source count", 1, analyzer.ReportSQLSources.Count);
					AssertEquals("Data source 1 table name", "foo:bar(<Param 1 name>, <Param 2 name>)", analyzer.ReportSQLSources[0].TableNameAndSelectStatement);
					AssertEquals("Data source 1 param count", 2, analyzer.DataSourceParameters.Count);
					AssertEquals("Data source 1 param 1 name", "PARAM 1 NAME", analyzer.DataSourceParameters[0]);
					AssertEquals("Data source 1 param 2 name", "PARAM 2 NAME", analyzer.DataSourceParameters[1]);
				}
			}
		}

		public void TestAddDataSourceWithTheSameParametersButDifferentUperLowerCase()
		{
			using (var report = new Report(pack, TestReport))
			{
				var analyzer = new ExposedReportAnalyser(report);
				analyzer.ReportSQLSources.Clear();

				analyzer.ExposedAddDataSource(@"data:foo=bar(<Param 1 name>, <PARAM 1 name>)");
				AssertEquals("Report data source count", 1, analyzer.ReportSQLSources.Count);
				AssertEquals("Data source 1 table name", "foo:bar(<Param 1 name>, <PARAM 1 name>)", analyzer.ReportSQLSources[0].TableNameAndSelectStatement);
				AssertEquals("Data source 1 param count", 1, analyzer.DataSourceParameters.Count);
				AssertEquals("Data source 1 param 1 name", "PARAM 1 NAME", analyzer.DataSourceParameters[0]);
			}
		}

		public void TestMainIDIsError()
		{
			using (var report = new Report(pack, TestReport))
			{
				var analyzer = new ExposedReportAnalyser(report);

				analyzer.ReportSQLSources.Clear();

				analyzer.ExposedAddDataSource(@"data:foo=bar(<MainID>)");
				AssertEquals("Report.Errors", @"Severity: [Error] Message: [This is an old style template, please replace 'MainID'.] Cell: [A1] Sheetname: [Sheet1]",
											report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
			}
		}

		public void TestNoDuplicateDataSourceParameters()
		{
			using (var report = new Report(pack, TestReport))
			{
				var analyzer = new ExposedReportAnalyser(report);

				analyzer.ReportSQLSources.Clear();
				analyzer.ExposedAddDataSource(@"data:foo=bar(<bingo>, <bingo>, <baz>)");
				analyzer.ExposedAddDataSource(@"data:oof=bar(<bingo>, <bingo>, <balalal>)");

				AssertEquals("Report data source count", 2, analyzer.ReportSQLSources.Count);
				AssertEquals("Should not duplicate parameters", 3, analyzer.DataSourceParameters.Count);
			}
		}

		public void TestTrailingFormFeedNotSpecified()
		{
			using (var rpt = new Report(pack, NewStyleTemplate))
			{
				rpt.PrepareForRender();
				AssertEquals("Trailing form feed not specified", 0, rpt.Analyser.Config.TrailingFormFeedLengthIn360thsOfAnInch);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTrailingFormFeed()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TrailingFormFeed.xls", TestFilesSubFolder.ReportTestFiles);
			using (var rpt = new Report(pack, excelTemplate))
			{
				rpt.PrepareForRender();
				AssertEquals("Trailing form feed", 1234, rpt.Analyser.Config.TrailingFormFeedLengthIn360thsOfAnInch);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFilterAndSortErrorsAreCollected()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("FilterAndSortErrors.xls", TestFilesSubFolder.ReportTestFiles);
			using (var rpt = new Report(pack, excelTemplate))
			{
				rpt.PrepareForRender();

				AssertEquals("Report.Errors", @"
Severity: [Error] Message: [Error Building Filters from Tree: There is no ""type"" under the ""Invalid syntax!"" block] Cell: [A1] Sheetname: [Filters]
Severity: [Error] Message: [Error Building Sort Orders from Tree: ""Also invalid syntax!"" should have one and only one item under it] Cell: [A1] Sheetname: [Sort]
".Trim(), rpt.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
			}
		}

		public void TestReadSpecificParameterValueFromConfigArea()
		{
			var templateContents =
				@"{A}-[#config]
{A}-[ShipmentNameOverride=Test]
{A}-[DisableTranslate]
{A}-[#EndOfReport]";

			using (var report = new Report(DocumentPack.EmptyPack, DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, templateContents)))
			{
				report.PrepareForRender();
				var analyser = report.Analyser;
				var shipmentNameOverride = analyser.ReadSpecificParameterValueFromConfigArea(report.WorkSheetCurrentlyBeingProcessed, "ShipmentNameOverride=");
				AssertEquals("Test", shipmentNameOverride);

				var disableTranslate = analyser.ReadSpecificParameterValueFromConfigArea(report.WorkSheetCurrentlyBeingProcessed, "DisableTranslate");
				AssertEquals("DisableTranslate", disableTranslate);

				var invalidParameterValue = analyser.ReadSpecificParameterValueFromConfigArea(report.WorkSheetCurrentlyBeingProcessed, "Blablabla");
				AssertEquals("", invalidParameterValue);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLastRowOfTemplateForBookingSummaryDoc()
		{
			AssertLastRowOfTemplate("BookingSummaryDoc.xls", 53);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLastRowOfTemplateForNewStyleTemplate()
		{
			AssertLastRowOfTemplate("NewStyleTemplate.xls", 60);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLastRowOfTemplateForBookingConfirmationDoc()
		{
			AssertLastRowOfTemplate("BookingConfirmationDoc.xls", 33);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLastRowOfTemplateForXLSXTemplate()
		{
			AssertLastRowOfTemplate("EmptyAndValidTemplate.xlsx", 5);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLastRowOfTemplateForXLSXTemplate_WhenLastRowOfTemplateIsBeyondMaxRowSupportedByExcel97_2003()
		{
			AssertLastRowOfTemplate("EmptyAndValidTemplateWithEndOfReportBeyondMaxRowSupportedByExcel97_2003.xlsx", 1000000);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataContext()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("DataContext - Shipment.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(pack, excelTemplate))
			{
				report.PrepareForRender();
				AssertEquals(Core.Constants.DataContext.Shipment, report.Analyser.Config.DataContextValue);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBackPage()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("BackPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				AssertNotNull(report.Analyser.BackPage);
				AssertEquals(60, report.Analyser.BackPage.StartingRow);
				AssertEquals(75, report.Analyser.BackPage.End);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConstants()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("Constants.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				AssertNotNull(report.ConstantsSheet);
				AssertEquals(8, report.TemplateDefinedConstants.Count);
				AssertEquals(10, report.TemplateDefinedConstants["FistParam"]);
				AssertEquals(112, report.TemplateDefinedConstants["Second param"]);
				AssertEquals("blah", report.TemplateDefinedConstants["Third param"]);

				AssertEquals("Constants from Constants sheet only.", 3, report.SheetDefinedConstants.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddNodeToConstants()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("Constants.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				var analyzer = new ExposedReportAnalyser(report);

				report.PrepareForRender();
				AssertNotNull(report.ConstantsSheet);
				AssertEquals(8, report.TemplateDefinedConstants.Count);

				StringTreeNode node = new StringTreeNode();
				node.Value = "bob";
				StringTreeNode childNode = new StringTreeNode();
				childNode.Value = "the builder";
				node.Children.Add(childNode);
				analyzer.ExposedAddNodeToConstantsIfNotAlreadyAdded(node);
				AssertEquals(9, report.TemplateDefinedConstants.Count);
				AssertEquals("the builder", report.TemplateDefinedConstants["bob"]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportNameAddedToConstants()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("Constants.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(pack, excelTemplate, "Pre-Alert", null, false))
			{
				report.PrepareForRender();
				Assert(report.TemplateDefinedConstants.ContainsKey(DocumentEngineIntegration.Constants.TemplateDefined.ReportName));
				AssertEquals("Pre-Alert", Utilities.GetStringFromObject(report.TemplateDefinedConstants[DocumentEngineIntegration.Constants.TemplateDefined.ReportName]));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentDirectionConstants()
		{
			ExcelTemplateForUnitTesting excelTemplate1 = new ExcelTemplateForUnitTesting("Constants.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(pack, excelTemplate1, null, "Test Report", null, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				Assert(report.TemplateDefinedConstants.ContainsKey(DocumentEngineIntegration.Constants.TemplateDefined.DocumentDirection));
				AssertEquals("ANY", Utilities.GetStringFromObject(report.TemplateDefinedConstants[DocumentEngineIntegration.Constants.TemplateDefined.DocumentDirection]));
			}

			ExcelTemplateForUnitTesting excelTemplate2 = new ExcelTemplateForUnitTesting("Constants.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(pack, excelTemplate2, null, "Test Report", null, DocumentDirection.DEP, false))
			{
				report.PrepareForRender();
				Assert(report.TemplateDefinedConstants.ContainsKey(DocumentEngineIntegration.Constants.TemplateDefined.DocumentDirection));
				AssertEquals("DEP", Utilities.GetStringFromObject(report.TemplateDefinedConstants[DocumentEngineIntegration.Constants.TemplateDefined.DocumentDirection]));
			}

			ExcelTemplateForUnitTesting excelTemplate3 = new ExcelTemplateForUnitTesting("Constants.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(pack, excelTemplate3, null, "Test Report", null, DocumentDirection.ARV, false))
			{
				report.PrepareForRender();
				Assert(report.TemplateDefinedConstants.ContainsKey(DocumentEngineIntegration.Constants.TemplateDefined.DocumentDirection));
				AssertEquals("ARV", Utilities.GetStringFromObject(report.TemplateDefinedConstants[DocumentEngineIntegration.Constants.TemplateDefined.DocumentDirection]));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestContactTypeConstants()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("Constants.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, "Test", ContactType.Consignee, true))
			{
				rpt.PrepareForRender();
				Assert(rpt.TemplateDefinedConstants.ContainsKey(DocumentEngineIntegration.Constants.TemplateDefined.ContactType));
				AssertEquals(ContactType.Consignee.ToString(), Utilities.GetStringFromObject(rpt.TemplateDefinedConstants[DocumentEngineIntegration.Constants.TemplateDefined.ContactType]));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestContactTypeConstants_CoverSheet()
		{
			StmMenuItem menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Parent Menu Name";
			menu.SU_ContactType = ContactType.Consignee.ToString();
			pack = new DocumentPack(menu);

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("Constants.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, new DataProviderList(new DeliveryInstructions()), "Anything", null, DocumentDirection.ANY, true))
			{
				rpt.PrepareForRender();
				AssertEquals(ContactType.Consignee.ToString(), Utilities.GetStringFromObject(rpt.TemplateDefinedConstants[DocumentEngineIntegration.Constants.TemplateDefined.ContactType]));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMenuTitleConstants()
		{
			StmMenuItem menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Menu Name";
			pack = new DocumentPack(menu);

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("Constants.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, "Test", ContactType.Consignee, true))
			{
				rpt.PrepareForRender();
				Assert(rpt.TemplateDefinedConstants.ContainsKey(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle));
				AssertEquals("Menu Name", Utilities.GetStringFromObject(rpt.TemplateDefinedConstants[DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle]));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMenuItemPKConstants()
		{
			StmMenuItem menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Menu Name";
			pack = new DocumentPack(menu);

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("Constants.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, "Test", ContactType.Consignee, true))
			{
				rpt.PrepareForRender();
				Assert(rpt.TemplateDefinedConstants.ContainsKey(DocumentEngineIntegration.Constants.TemplateDefined.MenuItemPK));
				AssertEquals(menu.PK.ToString(), Utilities.GetStringFromObject(rpt.TemplateDefinedConstants[DocumentEngineIntegration.Constants.TemplateDefined.MenuItemPK]));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestContactOrganisationPKConstants()
		{
			StmMenuItem menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Menu Name";
			pack = new DocumentPack(menu);

			ExcelTemplateForUnitTesting excelTemplate1 = new ExcelTemplateForUnitTesting("Constants.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate1, "Test", ContactType.Consignee, true))
			{
				rpt.PrepareForRender();
				Assert(rpt.TemplateDefinedConstants.ContainsKey(DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK));
				AssertEquals("Contact Org PK", ZGuid.Empty.ToString(), Utilities.GetStringFromObject(rpt.TemplateDefinedConstants[DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK]));
			}

			OrgHeader contactOrg = OrgHeader.New(Factory);
			pack.Organisation = contactOrg;

			ExcelTemplateForUnitTesting excelTemplate2 = new ExcelTemplateForUnitTesting("Constants.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate2, "Test", ContactType.Consignee, true))
			{
				rpt.PrepareForRender();
				Assert(rpt.TemplateDefinedConstants.ContainsKey(DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK));
				AssertEquals("Contact Org PK not empty", contactOrg.PK.ToString(), Utilities.GetStringFromObject(rpt.TemplateDefinedConstants[DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK]));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliveryModeConstants()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("Constants.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, "Test", ContactType.Consignee, true))
			{
				rpt.PrintCopyType = PrintCopyType.EML;
				rpt.PrepareForRender();
				Assert(rpt.TemplateDefinedConstants.ContainsKey(DocumentEngineIntegration.Constants.TemplateDefined.DeliveryMode));
				AssertEquals("Delivery Mode constants", nameof(PrintCopyType.EML), Utilities.GetStringFromObject(rpt.TemplateDefinedConstants[DocumentEngineIntegration.Constants.TemplateDefined.DeliveryMode]));
			}
		}

		void AddAdditionalTag(Report rpt)
		{
			rpt.WorkSheetCurrentlyBeingProcessed[40, 0] = "#SectionPageHeader";
			rpt.WorkSheetCurrentlyBeingProcessed[41, 0] = "#SectionBody:Data=Lines";
			rpt.WorkSheetCurrentlyBeingProcessed[42, 0] = "#SectionPageFooter";
			rpt.WorkSheetCurrentlyBeingProcessed[43, 0] = "#PageFooter";
			rpt.WorkSheetCurrentlyBeingProcessed[44, 0] = "#DocumentFooter";
			rpt.WorkSheetCurrentlyBeingProcessed[45, 0] = "#ENDOFREPORT";
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithoutElse_TrueExpression()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "50";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "70";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();
				AssertEquals("20", rpt.WorkSheetCurrentlyBeingProcessed[2, 0]);
				AssertEquals("40", rpt.WorkSheetCurrentlyBeingProcessed[3, 0]);
				AssertEquals("50", rpt.WorkSheetCurrentlyBeingProcessed[4, 0]);
				AssertEquals("70", rpt.WorkSheetCurrentlyBeingProcessed[5, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithoutElse_ManyPart()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "50";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "70";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[9, 0] = "90";

				rpt.WorkSheetCurrentlyBeingProcessed[10, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[11, 0] = "110";
				rpt.WorkSheetCurrentlyBeingProcessed[12, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[13, 0] = "130";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();
				AssertEquals("20", rpt.WorkSheetCurrentlyBeingProcessed[2, 0]);
				AssertEquals("40", rpt.WorkSheetCurrentlyBeingProcessed[3, 0]);
				AssertEquals("50", rpt.WorkSheetCurrentlyBeingProcessed[4, 0]);
				AssertEquals("70", rpt.WorkSheetCurrentlyBeingProcessed[5, 0]);
				AssertEquals("110", rpt.WorkSheetCurrentlyBeingProcessed[6, 0]);
				AssertEquals("130", rpt.WorkSheetCurrentlyBeingProcessed[7, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithoutElse_ManyPart_Withoutspace()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "50";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "80";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "90";

				rpt.WorkSheetCurrentlyBeingProcessed[10, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[11, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[12, 0] = "120";
				rpt.WorkSheetCurrentlyBeingProcessed[13, 0] = "130";
				rpt.WorkSheetCurrentlyBeingProcessed[14, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[15, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[16, 0] = "160";
				rpt.WorkSheetCurrentlyBeingProcessed[17, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[18, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[19, 0] = "#ENDIF";

				rpt.WorkSheetCurrentlyBeingProcessed[20, 0] = "200";
				rpt.WorkSheetCurrentlyBeingProcessed[21, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[22, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[23, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[24, 0] = "240";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();
				AssertEquals("20", rpt.WorkSheetCurrentlyBeingProcessed[2, 0]);
				AssertEquals("40", rpt.WorkSheetCurrentlyBeingProcessed[3, 0]);
				AssertEquals("50", rpt.WorkSheetCurrentlyBeingProcessed[4, 0]);
				AssertEquals("120", rpt.WorkSheetCurrentlyBeingProcessed[5, 0]);
				AssertEquals("130", rpt.WorkSheetCurrentlyBeingProcessed[6, 0]);
				AssertEquals("160", rpt.WorkSheetCurrentlyBeingProcessed[7, 0]);
				AssertEquals("200", rpt.WorkSheetCurrentlyBeingProcessed[8, 0]);
				AssertEquals("240", rpt.WorkSheetCurrentlyBeingProcessed[9, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithoutElse_FalseExpression()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "50";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "70";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();

				AssertEquals("20", rpt.WorkSheetCurrentlyBeingProcessed[2, 0]);
				AssertEquals("70", rpt.WorkSheetCurrentlyBeingProcessed[3, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithElse_TrueExpression()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "50";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "70";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[9, 0] = "90";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();

				AssertEquals("20", rpt.WorkSheetCurrentlyBeingProcessed[2, 0]);
				AssertEquals("40", rpt.WorkSheetCurrentlyBeingProcessed[3, 0]);
				AssertEquals("50", rpt.WorkSheetCurrentlyBeingProcessed[4, 0]);
				AssertEquals("90", rpt.WorkSheetCurrentlyBeingProcessed[5, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithElse_FalseExpression()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "50";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "70";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[9, 0] = "90";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();

				AssertEquals("20", rpt.WorkSheetCurrentlyBeingProcessed[2, 0]);
				AssertEquals("70", rpt.WorkSheetCurrentlyBeingProcessed[3, 0]);
				AssertEquals("90", rpt.WorkSheetCurrentlyBeingProcessed[4, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWith2IfAndElse_TF()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "50";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "70";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[9, 0] = "90";

				rpt.WorkSheetCurrentlyBeingProcessed[10, 0] = "100";
				rpt.WorkSheetCurrentlyBeingProcessed[11, 0] = "#IF(0>1)";
				rpt.WorkSheetCurrentlyBeingProcessed[12, 0] = "120";
				rpt.WorkSheetCurrentlyBeingProcessed[13, 0] = "130";
				rpt.WorkSheetCurrentlyBeingProcessed[14, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[15, 0] = "150";
				rpt.WorkSheetCurrentlyBeingProcessed[16, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[17, 0] = "170";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();

				AssertEquals("20", rpt.WorkSheetCurrentlyBeingProcessed[2, 0]);
				AssertEquals("40", rpt.WorkSheetCurrentlyBeingProcessed[3, 0]);
				AssertEquals("50", rpt.WorkSheetCurrentlyBeingProcessed[4, 0]);
				AssertEquals("90", rpt.WorkSheetCurrentlyBeingProcessed[5, 0]);
				AssertEquals("100", rpt.WorkSheetCurrentlyBeingProcessed[6, 0]);
				AssertEquals("150", rpt.WorkSheetCurrentlyBeingProcessed[7, 0]);
				AssertEquals("170", rpt.WorkSheetCurrentlyBeingProcessed[8, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWith2IfAndElse_FT()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "50";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "70";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[9, 0] = "90";

				rpt.WorkSheetCurrentlyBeingProcessed[10, 0] = "100";
				rpt.WorkSheetCurrentlyBeingProcessed[11, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[12, 0] = "120";
				rpt.WorkSheetCurrentlyBeingProcessed[13, 0] = "130";
				rpt.WorkSheetCurrentlyBeingProcessed[14, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[15, 0] = "150";
				rpt.WorkSheetCurrentlyBeingProcessed[16, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[17, 0] = "170";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();

				AssertEquals("20", rpt.WorkSheetCurrentlyBeingProcessed[2, 0]);
				AssertEquals("70", rpt.WorkSheetCurrentlyBeingProcessed[3, 0]);
				AssertEquals("90", rpt.WorkSheetCurrentlyBeingProcessed[4, 0]);
				AssertEquals("100", rpt.WorkSheetCurrentlyBeingProcessed[5, 0]);
				AssertEquals("120", rpt.WorkSheetCurrentlyBeingProcessed[6, 0]);
				AssertEquals("130", rpt.WorkSheetCurrentlyBeingProcessed[7, 0]);
				AssertEquals("170", rpt.WorkSheetCurrentlyBeingProcessed[8, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithNestedIf_1()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "60";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "80";
				rpt.WorkSheetCurrentlyBeingProcessed[9, 0] = "#ENDIF";

				rpt.WorkSheetCurrentlyBeingProcessed[10, 0] = "100";
				rpt.WorkSheetCurrentlyBeingProcessed[11, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[12, 0] = "120";
				rpt.WorkSheetCurrentlyBeingProcessed[13, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[14, 0] = "140";
				rpt.WorkSheetCurrentlyBeingProcessed[15, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[16, 0] = "160";
				rpt.WorkSheetCurrentlyBeingProcessed[17, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[18, 0] = "180";
				rpt.WorkSheetCurrentlyBeingProcessed[19, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[20, 0] = "200";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();

				AssertEquals("20", rpt.WorkSheetCurrentlyBeingProcessed[2, 0]);
				AssertEquals("40", rpt.WorkSheetCurrentlyBeingProcessed[3, 0]);
				AssertEquals("80", rpt.WorkSheetCurrentlyBeingProcessed[4, 0]);
				AssertEquals("100", rpt.WorkSheetCurrentlyBeingProcessed[5, 0]);
				AssertEquals("200", rpt.WorkSheetCurrentlyBeingProcessed[6, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithNestedIf_2()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "60";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "80";
				rpt.WorkSheetCurrentlyBeingProcessed[9, 0] = "#ENDIF";

				rpt.WorkSheetCurrentlyBeingProcessed[10, 0] = "100";
				rpt.WorkSheetCurrentlyBeingProcessed[11, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[12, 0] = "120";
				rpt.WorkSheetCurrentlyBeingProcessed[13, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[14, 0] = "140";
				rpt.WorkSheetCurrentlyBeingProcessed[15, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[16, 0] = "160";
				rpt.WorkSheetCurrentlyBeingProcessed[17, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[18, 0] = "180";
				rpt.WorkSheetCurrentlyBeingProcessed[19, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[20, 0] = "200";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();

				AssertEquals("20", rpt.WorkSheetCurrentlyBeingProcessed[2, 0]);
				AssertEquals("120", rpt.WorkSheetCurrentlyBeingProcessed[3, 0]);
				AssertEquals("140", rpt.WorkSheetCurrentlyBeingProcessed[4, 0]);
				AssertEquals("180", rpt.WorkSheetCurrentlyBeingProcessed[5, 0]);
				AssertEquals("200", rpt.WorkSheetCurrentlyBeingProcessed[6, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithNestedIf_3()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "60";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "80";
				rpt.WorkSheetCurrentlyBeingProcessed[9, 0] = "#ENDIF";

				rpt.WorkSheetCurrentlyBeingProcessed[10, 0] = "100";
				rpt.WorkSheetCurrentlyBeingProcessed[11, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[12, 0] = "120";
				rpt.WorkSheetCurrentlyBeingProcessed[13, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[14, 0] = "140";
				rpt.WorkSheetCurrentlyBeingProcessed[15, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[16, 0] = "160";
				rpt.WorkSheetCurrentlyBeingProcessed[17, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[18, 0] = "180";
				rpt.WorkSheetCurrentlyBeingProcessed[19, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[20, 0] = "200";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();

				AssertEquals("20", rpt.WorkSheetCurrentlyBeingProcessed[2, 0]);
				AssertEquals("40", rpt.WorkSheetCurrentlyBeingProcessed[3, 0]);
				AssertEquals("60", rpt.WorkSheetCurrentlyBeingProcessed[4, 0]);
				AssertEquals("100", rpt.WorkSheetCurrentlyBeingProcessed[5, 0]);
				AssertEquals("200", rpt.WorkSheetCurrentlyBeingProcessed[6, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithNestedIf_4()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "60";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "70";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "80";
				rpt.WorkSheetCurrentlyBeingProcessed[9, 0] = "#ENDIF";

				rpt.WorkSheetCurrentlyBeingProcessed[10, 0] = "100";
				rpt.WorkSheetCurrentlyBeingProcessed[11, 0] = "110";
				rpt.WorkSheetCurrentlyBeingProcessed[12, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[13, 0] = "130";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();

				AssertEquals("20", rpt.WorkSheetCurrentlyBeingProcessed[2, 0]);
				AssertEquals("40", rpt.WorkSheetCurrentlyBeingProcessed[3, 0]);
				AssertEquals("100", rpt.WorkSheetCurrentlyBeingProcessed[4, 0]);
				AssertEquals("110", rpt.WorkSheetCurrentlyBeingProcessed[5, 0]);
				AssertEquals("130", rpt.WorkSheetCurrentlyBeingProcessed[6, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithNestedIf_5()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "60";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "80";
				rpt.WorkSheetCurrentlyBeingProcessed[9, 0] = "#IF(1>0)";

				rpt.WorkSheetCurrentlyBeingProcessed[10, 0] = "100";
				rpt.WorkSheetCurrentlyBeingProcessed[11, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[12, 0] = "120";
				rpt.WorkSheetCurrentlyBeingProcessed[13, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[14, 0] = "140";
				rpt.WorkSheetCurrentlyBeingProcessed[15, 0] = "150";
				rpt.WorkSheetCurrentlyBeingProcessed[16, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[17, 0] = "170";
				rpt.WorkSheetCurrentlyBeingProcessed[18, 0] = "180";
				rpt.WorkSheetCurrentlyBeingProcessed[19, 0] = "#IF(1>0)";

				rpt.WorkSheetCurrentlyBeingProcessed[20, 0] = "200";
				rpt.WorkSheetCurrentlyBeingProcessed[21, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[22, 0] = "220";
				rpt.WorkSheetCurrentlyBeingProcessed[23, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[24, 0] = "240";
				rpt.WorkSheetCurrentlyBeingProcessed[25, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[26, 0] = "260";
				rpt.WorkSheetCurrentlyBeingProcessed[27, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[28, 0] = "280";
				rpt.WorkSheetCurrentlyBeingProcessed[29, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[30, 0] = "300";

				rpt.WorkSheetCurrentlyBeingProcessed[31, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[32, 0] = "320";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();

				AssertEquals("20", rpt.WorkSheetCurrentlyBeingProcessed[2, 0]);
				AssertEquals("40", rpt.WorkSheetCurrentlyBeingProcessed[3, 0]);
				AssertEquals("80", rpt.WorkSheetCurrentlyBeingProcessed[4, 0]);
				AssertEquals("100", rpt.WorkSheetCurrentlyBeingProcessed[5, 0]);
				AssertEquals("140", rpt.WorkSheetCurrentlyBeingProcessed[6, 0]);
				AssertEquals("150", rpt.WorkSheetCurrentlyBeingProcessed[7, 0]);
				AssertEquals("170", rpt.WorkSheetCurrentlyBeingProcessed[8, 0]);
				AssertEquals("180", rpt.WorkSheetCurrentlyBeingProcessed[9, 0]);
				AssertEquals("200", rpt.WorkSheetCurrentlyBeingProcessed[10, 0]);
				AssertEquals("240", rpt.WorkSheetCurrentlyBeingProcessed[11, 0]);
				AssertEquals("260", rpt.WorkSheetCurrentlyBeingProcessed[12, 0]);
				AssertEquals("280", rpt.WorkSheetCurrentlyBeingProcessed[13, 0]);
				AssertEquals("320", rpt.WorkSheetCurrentlyBeingProcessed[14, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithNestedIf_WithoutSpace()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "50";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "80";
				rpt.WorkSheetCurrentlyBeingProcessed[9, 0] = "#ELSE";

				rpt.WorkSheetCurrentlyBeingProcessed[10, 0] = "100";
				rpt.WorkSheetCurrentlyBeingProcessed[11, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[12, 0] = "120";
				rpt.WorkSheetCurrentlyBeingProcessed[13, 0] = "130";
				rpt.WorkSheetCurrentlyBeingProcessed[14, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[15, 0] = "150";
				rpt.WorkSheetCurrentlyBeingProcessed[16, 0] = "160";
				rpt.WorkSheetCurrentlyBeingProcessed[17, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[18, 0] = "180";
				rpt.WorkSheetCurrentlyBeingProcessed[19, 0] = "#ELSE";

				rpt.WorkSheetCurrentlyBeingProcessed[20, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[21, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[22, 0] = "220";
				rpt.WorkSheetCurrentlyBeingProcessed[23, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[24, 0] = "240";
				rpt.WorkSheetCurrentlyBeingProcessed[25, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[26, 0] = "260";
				rpt.WorkSheetCurrentlyBeingProcessed[27, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[28, 0] = "280";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();

				AssertEquals("20", rpt.WorkSheetCurrentlyBeingProcessed[2, 0]);
				AssertEquals("80", rpt.WorkSheetCurrentlyBeingProcessed[3, 0]);
				AssertEquals("120", rpt.WorkSheetCurrentlyBeingProcessed[4, 0]);
				AssertEquals("130", rpt.WorkSheetCurrentlyBeingProcessed[5, 0]);
				AssertEquals("150", rpt.WorkSheetCurrentlyBeingProcessed[6, 0]);
				AssertEquals("160", rpt.WorkSheetCurrentlyBeingProcessed[7, 0]);
				AssertEquals("220", rpt.WorkSheetCurrentlyBeingProcessed[8, 0]);
				AssertEquals("240", rpt.WorkSheetCurrentlyBeingProcessed[9, 0]);
				AssertEquals("280", rpt.WorkSheetCurrentlyBeingProcessed[10, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithNestedIf_Exception_1()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[0, 0] = "#Config";
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "60";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "70";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "80";
				rpt.WorkSheetCurrentlyBeingProcessed[9, 0] = "#ENDIF";

				rpt.WorkSheetCurrentlyBeingProcessed[10, 0] = "100";
				rpt.WorkSheetCurrentlyBeingProcessed[11, 0] = "110";
				rpt.WorkSheetCurrentlyBeingProcessed[12, 0] = "120";
				rpt.WorkSheetCurrentlyBeingProcessed[13, 0] = "130";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();

				AssertEquals("Report.Errors", "Severity: [Error] Message: [#IF in row number 3 does not have #ENDIF] Cell: [A4]",
										rpt.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithNestedIf_Exception_2()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[0, 0] = "#Config";
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "#IF(1>0)";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "60";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "70";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "80";
				rpt.WorkSheetCurrentlyBeingProcessed[9, 0] = "#ENDIF";

				rpt.WorkSheetCurrentlyBeingProcessed[10, 0] = "100";
				rpt.WorkSheetCurrentlyBeingProcessed[11, 0] = "110";
				rpt.WorkSheetCurrentlyBeingProcessed[12, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[13, 0] = "130";
				rpt.WorkSheetCurrentlyBeingProcessed[14, 0] = "#ELSE";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();

				AssertEquals("Report.Errors", "Severity: [Error] Message: [#ELSE in row number 14 does not have #IF] Cell: [A15]",
										rpt.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsWithNestedIf_Exception_3()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed.ClearRange(0, 0, 100, 100);
				rpt.WorkSheetCurrentlyBeingProcessed[0, 0] = "#Config";
				rpt.WorkSheetCurrentlyBeingProcessed[2, 0] = "20";
				rpt.WorkSheetCurrentlyBeingProcessed[3, 0] = "30";
				rpt.WorkSheetCurrentlyBeingProcessed[4, 0] = "40";
				rpt.WorkSheetCurrentlyBeingProcessed[5, 0] = "#IF(1<0)";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "60";
				rpt.WorkSheetCurrentlyBeingProcessed[7, 0] = "#ELSE";
				rpt.WorkSheetCurrentlyBeingProcessed[8, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[9, 0] = "90";

				rpt.WorkSheetCurrentlyBeingProcessed[10, 0] = "100";
				rpt.WorkSheetCurrentlyBeingProcessed[11, 0] = "110";
				rpt.WorkSheetCurrentlyBeingProcessed[12, 0] = "#ENDIF";
				rpt.WorkSheetCurrentlyBeingProcessed[13, 0] = "130";

				AddAdditionalTag(rpt);

				rpt.PrepareForRender();

				AssertEquals("Report.Errors", "Severity: [Error] Message: [#ENDIF in row number 12 does not have #IF] Cell: [A13]",
										rpt.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsBasedOnSortMacro()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("Whs Stock Movements.xls", TestFilesSubFolder.ReportTestFiles);
			var pack = new DocumentPack(Factory.New<StmMenuItem>());

			using (var report = new Report(pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();

				AssertEquals("Sort orders loaded and condition was satisfied", "#GroupBy:ReportData.WarehouseName+ReportData.ClientCode+ReportData.FinalisedDate+ReportData.Product", report.WorkSheetCurrentlyBeingProcessed[30, 0]);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1136:DoNotUseSystemRuntimeSerializationFormattersBinary", Justification = "WI00700503 - Pending migration")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideConditionalRowsBasedOnSortMacroAndGroupByMacroAndDeserialisedReport()
		{
			var pack = new DocumentPack(Factory.New<StmMenuItem>());
			Report deserialisedReport;

			var excelTemplate1 = new ExcelTemplateForUnitTesting("UsingSelectedSortOrderMacroAndSelectedGroupbyMacroInIf.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(pack, excelTemplate1))
			{
				report.PrepareForRender();
				report.SortOrderCollection.SelectedOrder = report.SortOrderCollection[2];
				report.GroupByCollection.SelectedGroupBy = report.GroupByCollection[1];

				var result = JsonConverterHelper.Serialize(report);
				deserialisedReport = JsonConverterHelper.Deserialize<Report>(result);
			}

			var excelTemplate2 = new ExcelTemplateForUnitTesting("UsingSelectedSortOrderMacroAndSelectedGroupbyMacroInIf.xls", TestFilesSubFolder.ReportTestFiles);
			using (deserialisedReport)
			using (var report = new Report(pack, excelTemplate2))
			{
				report.DeserializedReport = deserialisedReport;
				report.PrepareForRender();
				AssertEquals("Sort orders loaded and condition was satisfied", "#GroupBy:ReportData.DocketType+ReportData.Product+ReportData.FinalisedDate", report.WorkSheetCurrentlyBeingProcessed[30, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupByCollection()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupByPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();
				AssertEquals(4, rpt.GroupByCollection.Count);
				AssertEquals("Only RL_HasAirport", rpt.GroupByCollection[0].DisplayName);
				AssertEquals("Lines.RL_HasAirport", rpt.GroupByCollection[0].FieldList);
				AssertEquals(false, rpt.GroupByCollection[0].Selected);

				AssertEquals("All", rpt.GroupByCollection[1].DisplayName);
				AssertEquals("Lines.RL_HasAirport,Lines.RL_RN_NKCountryCode", rpt.GroupByCollection[1].FieldList);
				AssertEquals(false, rpt.GroupByCollection[1].Selected);

				AssertEquals("None", rpt.GroupByCollection[2].DisplayName);
				AssertEquals("", rpt.GroupByCollection[2].FieldList);
				AssertEquals(true, rpt.GroupByCollection[2].Selected);

				AssertEquals("Country", rpt.GroupByCollection[3].DisplayName);
				AssertEquals("Lines.RL_RN_NKCountryCode", rpt.GroupByCollection[3].FieldList);
				AssertEquals(false, rpt.GroupByCollection[3].Selected);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoesntReReadOptionalTemplatesOnceRead()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesForOptonalRenderTest.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.OptionalTemplateSheetCollection.Add("Test Sheet");
				AssertEquals("OptionalTemplateCollections has only one element", 1, rpt.OptionalTemplateSheetCollection.Count);

				rpt.PrepareForRender();
				AssertEquals("Optional template collection now has 2 elements", 2, rpt.OptionalTemplateSheetCollection.Count);
				AssertEquals("Optional template collection does not contain Test Sheet", false, rpt.OptionalTemplateSheetCollection.Contains("Test Sheet"));

				rpt.OptionalTemplateSheetCollection.Add("Test Sheet");
				AssertEquals("Optional template collection contains Test Sheet again", true, rpt.OptionalTemplateSheetCollection.Contains("Test Sheet"));

				rpt.Analyser.Analyse();
				AssertEquals("Optional template sheet collection still contains Test sheet because analyse didn't re-read", true, rpt.OptionalTemplateSheetCollection.Contains("Test Sheet"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOptionalTemplateSheetErrorsCaptured()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalTemplatesMorethanOnce.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.OptionalTemplateSheetCollection.Add("Test Sheet");
				rpt.PrepareForRender();
				AssertEquals("Analyser should have error", true, rpt.ErrorManager.ToString().Contains("Sheet1 is specified on the optional templates sheet more than once."));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMandatoryTemplateSheetsIncludedInAtLeastOneOfValidationForOptionalTemplates()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalTemplates.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();
				foreach (OptionalTemplateSheet sheet in rpt.OptionalTemplateSheetCollection)
				{
					sheet.Validate();
					AssertEquals("There's a mandatory sheet so should be valid", false, sheet.SelectedInfo.HasErrors());
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAllFilterFieldsAreRegisteredAsLinked()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("MultipleFiltersWithGroups.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				AssertEquals("Prerequisite: linked fields collection should be empty", 0, rpt.ColumnHeadingManager.LinkedFilterFields.Count);
				rpt.PrepareForRender();
				AssertEquals("Linked fields collection should contain all filters", 8, rpt.ColumnHeadingManager.LinkedFilterFields.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFiltersWithDefaultValuesAreCopiedToHeadingManager()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("ExactTextFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				AssertEquals("Prerequisite: filters collection should be empty", 0, rpt.FilterCollection.Count);
				rpt.PrepareForRender();
				AssertEquals("Filters collection should now load", 2, rpt.FilterCollection.Count);
				AssertEquals("Filters collection should be copied into heading manager (without column configuration field)", 1, rpt.ColumnHeadingManager.DefaultFilters.Count);
				AssertEquals(((ExactTextField)rpt.FilterCollection[1]).Value, ((ExactTextField)rpt.ColumnHeadingManager.DefaultFilters[0]).Value);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestColumnConfigurationFieldOnlyAddedForReports()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("ExactTextFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				AssertEquals("Prerequisite: filters collection should be empty", 0, rpt.FilterCollection.Count);
				rpt.PrepareForRender();
				AssertEquals("Count", 2, rpt.FilterCollection.Count);
				Assert("First filter should be ColumnConfigurationField", rpt.FilterCollection[0] is ColumnConfigurationField);
			}

			using (Report rpt = new Report(pack, excelTemplate, null, "Test Report", null, DocumentDirection.ANY, false))
			{
				rpt.ErrorManager.ClearErrors();
				AssertEquals("Prerequisite: filters collection should be empty", 0, rpt.FilterCollection.Count);
				rpt.PrepareForRender();
				AssertEquals("Count", 1, rpt.FilterCollection.Count);
				Assert("First filter should not be ColumnConfigurationField", !(rpt.FilterCollection[0] is ColumnConfigurationField));
			}
		}

		public void TestValidTurkishDocument()
		{
			using (Env.Instance.SetTemporaryUserContext(Env.Instance.CurrentUser.PK, Env.Instance.CurrentBranch.PK, Env.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Precondition: ConsumptionLogCreated", false, Env.Licence.LanguagePackLookup[Core.SharedConstants.Languages.Turkish].ConsumptionLogCreated);

				var templateContents =
	@"{A}-[#Config]
{A}-[#SectionBody]
{B}-[Hello]
{A}-[#EndOfReport]";

				var dummy = Factory.New<DummyBusinessObject>();
				var docDataProvider = BODocDataProvider.Get(dummy);

				using (var documentPack = new DocumentPack())
				{
					documentPack.Language = Core.SharedConstants.Languages.Turkish;
					using (var stream = new MemoryStream())
					using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, docDataProvider, stream, templateContents))
					{
						var renderer = new ReportRenderer(report);
						renderer.Render();
						Assert(report.ErrorManager.ToString(), !report.ErrorManager.HasErrors);
					}
				}

				AssertEquals("ConsumptionLogCreated", true, Env.Licence.LanguagePackLookup[Core.SharedConstants.Languages.Turkish].DocBuilderLanguageCheckpoint.ConsumptionLogCreated);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportShouldOnlyGetLocalizedDataWithReportStyle()
		{
			var testFile = new ExcelTemplateForUnitTesting("CombinedFilterSortGroupByOptional.xls", TestFilesSubFolder.ReportTestFiles);

			using (var reportWithDocumentStyle = new Report(pack, testFile, new DataProviderList(new DeliveryInstructions()), "Anything", null, DocumentDirection.ANY, true))
			{
				var documentTemplate = Factory.New<StmTemplateBase>();
				documentTemplate.SO_ExcelTemplatePath = testFile.TemplateSourceLocation;
				documentTemplate.SO_IsSystemDefined = true;
				reportWithDocumentStyle.StTemplate = documentTemplate;
				reportWithDocumentStyle.PrepareForRender();

				CombineAssertions("All DisplayNameLocalizedDatas should be null", () =>
				{
					AssertNull((reportWithDocumentStyle.FilterCollection[0] as FilterField).DisplayNameLocalizedData);
					AssertNull((reportWithDocumentStyle.FilterCollection[1] as MultipleSelectionLookup).Columns[0].CaptionLocalizedData);
					AssertNull(reportWithDocumentStyle.SortOrderCollection[0].DisplayNameLocalizedData);
					AssertNull(reportWithDocumentStyle.GroupByCollection[0].DisplayNameLocalizedData);
					AssertNull((reportWithDocumentStyle.OptionalTemplateSheetCollection.First() as OptionalTemplateSheet).DisplayNameLocalizedData);
				});
			}

			using (var reportWithReportStyle = new Report(pack, testFile))
			{
				var reportTemplate = Factory.New<StmTemplateBase>();
				reportTemplate.SO_ExcelTemplatePath = testFile.TemplateSourceLocation;
				reportTemplate.SO_IsSystemDefined = true;
				reportWithReportStyle.StTemplate = reportTemplate;
				reportWithReportStyle.PrepareForRender();

				CombineAssertions("All DisplayNameLocalizedDatas should not be null", () =>
				{
					AssertNotNull((reportWithReportStyle.FilterCollection[0] as FilterField).DisplayNameLocalizedData);
					AssertNotNull((reportWithReportStyle.FilterCollection[2] as MultipleSelectionLookup).Columns[0].CaptionLocalizedData);
					AssertNotNull(reportWithReportStyle.SortOrderCollection[0].DisplayNameLocalizedData);
					AssertNotNull(reportWithReportStyle.GroupByCollection[0].DisplayNameLocalizedData);
					AssertNotNull((reportWithReportStyle.OptionalTemplateSheetCollection.First() as OptionalTemplateSheet).DisplayNameLocalizedData);
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportShouldTranslateDescriptionPairListSupportField()
		{
			var testFile = new ExcelTemplateForUnitTesting("DescriptionPairListSupportFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var resourceStrings = Res.UseMockData())
			{
				var key1 = DocBuilderResourceStrings.GetKey("DescriptionPairListSupportFilter", DocBuilderResourceStrings.ReportLabelKeyPrefix, "Sea");
				var key2 = DocBuilderResourceStrings.GetKey("DescriptionPairListSupportFilter", DocBuilderResourceStrings.ReportLabelKeyPrefix, "Air");
				var key3 = DocBuilderResourceStrings.GetKey("DescriptionPairListSupportFilter", DocBuilderResourceStrings.ReportLabelKeyPrefix, "Allow Multiple Carrier");
				var key4 = DocBuilderResourceStrings.GetKey("DescriptionPairListSupportFilter", DocBuilderResourceStrings.ReportLabelKeyPrefix, "Description0");
				resourceStrings.Put(key1, new ResourceStringData(key1, "海运"));
				resourceStrings.Put(key2, new ResourceStringData(key2, "空运"));
				resourceStrings.Put(key3, new ResourceStringData(key3, "允许多个承运人"));
				resourceStrings.Put(key4, new ResourceStringData(key4, "描述1"));

				var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
				dummyBizo.Z0_FK_Code = "Code0";
				dummyBizo.Z0_Description = "Description0";
				Factory.Save();

				using (var reportWithReportStyle = new Report(pack, testFile))
				{
					var reportTemplate = Factory.New<StmTemplateBase>();
					reportTemplate.SO_ExcelTemplatePath = testFile.TemplateSourceLocation;
					reportTemplate.SO_IsSystemDefined = true;
					reportWithReportStyle.StTemplate = reportTemplate;
					reportWithReportStyle.PrepareForRender();

					CombineAssertions("All of the descriptions should be translated", () =>
					{
						AssertEquals("海运", ((IMultilingualDescription)((MultipleChoice)reportWithReportStyle.FilterCollection[1]).List[0]).MultilingualDescription);
						AssertEquals("空运", ((IMultilingualDescription)((MultipleChoice)reportWithReportStyle.FilterCollection[1]).List[1]).MultilingualDescription);
						AssertEquals("允许多个承运人", ((OptionGroup)reportWithReportStyle.FilterCollection[2]).DescriptionCodePairList[0].Description);
						AssertEquals("描述1", ((IMultilingualDescription)((CodeListMultipleChoice)reportWithReportStyle.FilterCollection[3]).List[0]).MultilingualDescription);
					});
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();
			pack = new DocumentPack();
			temporarilyUseMainConnection = Report.TemporarilyUseMainConnection();
		}

		IDisposable temporarilyUseMainConnection;
		DocumentPack pack;
		EmbeddedResourceRetriever embeddedResourceRetriever;

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
			temporarilyUseMainConnection?.Dispose();
		}

		ExcelTemplateForUnitTesting testReport;
		ExcelTemplateForUnitTesting TestReport
		{
			get
			{
				if (testReport == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					testReport = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return testReport;
			}
		}

		ExcelTemplateForUnitTesting newStyleTemplate;
		ExcelTemplateForUnitTesting NewStyleTemplate
		{
			get
			{
				if (newStyleTemplate == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
					newStyleTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return newStyleTemplate;
			}
		}

		void AssertHideColumnIfWithNonExistantOptionalColumnSuspendsErrorMessage(bool customized)
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opportunity = organisation.SalesOpportunities.AddNew();
			opportunity.P8_EstimatedValue = 12;
			opportunity.P8_OpportunityType = "DUD";
			Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("HideColumnIfWithNonExistantOptionalColumns.xls", TestFilesSubFolder.ReportTestFiles);
			excelTemplate.ContainsCustomisedSections = customized;
			using (var report = new Report(pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				using (var stream = new MemoryStream())
				{
					AssertEquals("Precondition - report.ErrorManager.HasErrors = false", false, report.ErrorManager.HasErrors);
					AssertEquals("report.Analyser.NonExistantOptionalColumnsContains(Wrong Column (8))", false, report.Analyser.NonExistantOptionalColumnsContains(8));
					AssertEquals("report.Analyser.NonExistantOptionalColumnsContains(\"<HeaderData.Type1>\")", false, report.Analyser.NonExistantOptionalColumnsContains(9));
					AssertEquals("report.Analyser.NonExistantOptionalColumnsContains(\"<HeaderData.Type2>\")", false, report.Analyser.NonExistantOptionalColumnsContains(10));
					AssertEquals("report.Analyser.NonExistantOptionalColumnsContains(Wrong Column (11))", false, report.Analyser.NonExistantOptionalColumnsContains(11));

					report.Save(stream);
					AssertEquals("ErrorManager.HasErrors = false", false, report.ErrorManager.HasErrors);
					AssertEquals("report.Analyser.NonExistantOptionalColumnsContains(Wrong Column (8))", false, report.Analyser.NonExistantOptionalColumnsContains(8));
					AssertEquals("report.Analyser.NonExistantOptionalColumnsContains(\"<HeaderData.Type1>\")", true, report.Analyser.NonExistantOptionalColumnsContains(9));
					AssertEquals("report.Analyser.NonExistantOptionalColumnsContains(\"<HeaderData.Type2>\")", true, report.Analyser.NonExistantOptionalColumnsContains(10));
					AssertEquals("report.Analyser.NonExistantOptionalColumnsContains(Wrong Column (11))", false, report.Analyser.NonExistantOptionalColumnsContains(11));

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", @"
{B}-[Test Report]
{D}-[Stage Desc.]   {E}-[Last Activity Date]   {F}-[Estimated Close Date]   {G}-[Objective]   {H}-[Objective Desc.]   {I}-[Estimated Value]
{G}-[DUD]   {I}-[12]
".Trim(), workSheet.ToString());
					}
				}
			}
		}

		void AssertLastRowOfTemplate(string fileName, int expectedLastRow)
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(fileName, TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(pack, excelTemplate))
			{
				report.PrepareForRender();
				AssertEquals(expectedLastRow, report.Analyser.LastRowOfTemplate);
			}
		}
	}
}
