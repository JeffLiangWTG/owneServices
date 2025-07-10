#if DEBUG
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating
{
	public class DocBuilderTemplateUpdater : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocBuilderTemplateUpdater(BusinessObjectFactory factory)
			: base(factory)
		{
			this.updateTemplateCommands = GetUpdateTemplateCommands();
		}

		readonly UpdateTemplateCommandCollection updateTemplateCommands;

		UpdateTemplateCommandCollection GetUpdateTemplateCommands()
		{
			var result = new UpdateTemplateCommandCollection(Factory);

			result.Add(new SplitDuplicateHeadersEnglish(Factory));
			result.Add(new SplitDuplicateShortModeDepartureArrivalHeadersEnglish(Factory));
			result.Add(new SplitDuplicateModeDepartureArrivalHeadersEnglish(Factory));
			result.Add(new SplitDuplicateShortHeadersEnglish(Factory));
			result.Add(new SplitDuplicateHBLContainerModeHeadersEnglish(Factory));
			result.Add(new SplitDuplicateServiceDocumentTitlesEnglish(Factory));
			result.Add(new SplitDuplicateShortModeDepartureArrivalHeadersGerman(Factory));

			return result;
		}

		public UpdateTemplateCommandCollection UpdateTemplateCommands
		{
			get { return updateTemplateCommands; }
		}

		public void Execute()
		{
			foreach (UpdateTemplateCommand command in UpdateTemplateCommands)
			{
				command.Execute();
			}
		}

		public void SaveDocBuilderTemplatesToFile()
		{
			SaveToFile(StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System, Enterprise.Core.Constants.Languages.English));
			SaveToFile(StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System, Enterprise.Core.Constants.Languages.German));
		}

#if DEBUG
		internal
#endif
		class SplitDuplicateHeadersEnglish : UpdateTemplateCommand
		{
			public SplitDuplicateHeadersEnglish(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override ZString Description
			{
				get { return "[ENG] Split duplicate headers into their own strip."; }
			}

			public override void Execute()
			{
				var template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System, Enterprise.Core.Constants.Languages.English);
				var manipulator = new ConfigurableTemplateManipulator(template);

				manipulator.SplitSectionAndConfigItems(
					"Header with Booking and BillOfLading numbers / Receival and HazReceival dates",
					3,
					"Document Title with Page Numbers",
					"Recipient with Booking + BillOfLading numbers + Receival + HazReceival dates");

				manipulator.SplitReplaceAndRemove(
					"Header with Booking and BillOfLading numbers / Receival dates",
					3,
					"Recipient with Booking + BillOfLading numbers + Receival dates",
					"Document Title with Page Numbers");

				manipulator.SplitReplaceAndRemove(
					"Header with Booking and BillOfLading numbers / HazReceival dates",
					3,
					"Recipient with Booking + BillOfLading numbers + HazReceival dates",
					"Document Title with Page Numbers");

				manipulator.SplitReplaceAndRemove(
					"Header with Booking number / Receival and HazReceival dates",
					3,
					"Recipient with Booking number + Receival + HazReceival dates",
					"Document Title with Page Numbers");

				manipulator.SplitReplaceAndRemove(
					"Header with Booking number / Receival dates",
					3,
					"Recipient with Booking number + Receival dates",
					"Document Title with Page Numbers");

				manipulator.SplitReplaceAndRemove(
					"Header with Booking number / HazReceival dates",
					3,
					"Recipient with Booking number + HazReceival dates",
					"Document Title with Page Numbers");

				manipulator.SplitReplaceAndRemove(
					"Header with Booking and BillOfLading numbers",
					3,
					"Recipient with Booking + BillOfLading numbers",
					"Document Title with Page Numbers");

				manipulator.SplitReplaceAndRemove(
					"Header with Booking number",
					3,
					"Recipient with Booking number",
					"Document Title with Page Numbers");
			}
		}

#if DEBUG
		internal
#endif
		class SplitDuplicateShortModeDepartureArrivalHeadersEnglish : UpdateTemplateCommand
		{
			public SplitDuplicateShortModeDepartureArrivalHeadersEnglish(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override ZString Description
			{
				get { return "[ENG] Split duplicate Mode + Departure/Arrival headers (Short) into their own strip."; }
			}

			public override void Execute()
			{
				var template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System, Enterprise.Core.Constants.Languages.English);
				var manipulator = new ConfigurableTemplateManipulator(template);

				manipulator.SplitSectionAndConfigItems(
					"Mode + Departure/Arrival Header",
					3,
					"Document Title with Mode + Departure/Arrival (Short)",
					"Recipient with Job Number");

				manipulator.SplitReplaceAndRemove(
					"Mode + Departure/Arrival Header + Available Date + Storage Date",
					3,
					"Recipient with Available Date + Storage Date",
					"Document Title with Mode + Departure/Arrival (Short)");

				manipulator.SplitReplaceAndRemove(
					"Mode + Departure/Arrival Header with Container Number + Available Date + Storage Date",
					3,
					"Recipient with Container Number + Available Date + Storage Date",
					"Document Title with Mode + Departure/Arrival (Short)");

				manipulator.SplitReplaceAndRemove(
					"Mode + Departure/Arrival Header with Container Number + Available Date + Storage Date (Minimal)",
					3,
					"Recipient with Container Number",
					"Document Title with Mode + Departure/Arrival (Short)");

				manipulator.SplitReplaceAndRemove(
					"Mode + Departure/Arrival but without Recipient Name and Address",
					3,
					"Header with Job Number + Secondary Number + Date",
					"Document Title with Mode + Departure/Arrival (Short)");

				manipulator.SplitReplaceAndRemove(
					"Mode + Departure/Arrival Header + Consignee + Terms",
					3,
					"Recipient with Consignee + Terms",
					"Document Title with Mode + Departure/Arrival (Short)");

				manipulator.SplitReplaceAndRemove(
					"Mode + Type + Style + Departure/Arrival but without Recipient Name and Address",
					3,
					"Header with Type + Style + Date",
					"Document Title with Mode + Departure/Arrival (Short)");
			}
		}

#if DEBUG
		internal
#endif
		class SplitDuplicateModeDepartureArrivalHeadersEnglish : UpdateTemplateCommand
		{
			public SplitDuplicateModeDepartureArrivalHeadersEnglish(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override ZString Description
			{
				get { return "[ENG] Split duplicate Mode + Departure/Arrival headers into their own strip."; }
			}

			public override void Execute()
			{
				var template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System, Enterprise.Core.Constants.Languages.English);
				var manipulator = new ConfigurableTemplateManipulator(template);

				manipulator.SplitSectionAndConfigItems(
					"Full report name with Mode + Departure/Arrival + Container Number + Available Date + Storage Date",
					3,
					"Document Title with Mode + Departure/Arrival",
					"Recipient with Container Number + Available Date + Storage Date (2)");

				manipulator.ChangeConfigItemToUseAnotherSectionAndRemoveOldSection(
					"Recipient with Container Number + Available Date + Storage Date (2)",
					"Recipient with Container Number + Available Date + Storage Date");

				manipulator.SplitReplaceAndRemove(
					"Full report name with Mode + Departure/Arrival + Container Number",
					3,
					"Recipient with Container Number (2)",
					"Document Title with Mode + Departure/Arrival");

				manipulator.ChangeConfigItemToUseAnotherSectionAndRemoveOldSection(
					"Recipient with Container Number (2)",
					"Recipient with Container Number");
			}
		}

#if DEBUG
		internal
#endif
		class SplitDuplicateShortHeadersEnglish : UpdateTemplateCommand
		{
			public SplitDuplicateShortHeadersEnglish(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override ZString Description
			{
				get { return "[ENG] Split duplicate short headers into their own strip."; }
			}

			public override void Execute()
			{
				var template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System, Enterprise.Core.Constants.Languages.English);
				var manipulator = new ConfigurableTemplateManipulator(template);

				manipulator.SplitSectionAndConfigItems(
					"Header",
					3,
					"Document Title with Page Numbers (Short)",
					"Recipient with Job Number (2)");

				manipulator.ChangeConfigItemToUseAnotherSectionAndRemoveOldSection(
					"Recipient with Job Number (2)",
					"Recipient with Job Number");

				manipulator.SplitReplaceAndRemove(
					"Header without Recipient Name and Address",
					3,
					"Header with Job Number",
					"Document Title with Page Numbers (Short)");

				manipulator.SplitReplaceAndRemove(
					"Header from Sundry Charges",
					3,
					"Recipient with Sundry Charges",
					"Document Title with Page Numbers (Short)");
			}
		}

#if DEBUG
		internal
#endif
		class SplitDuplicateHBLContainerModeHeadersEnglish : UpdateTemplateCommand
		{
			public SplitDuplicateHBLContainerModeHeadersEnglish(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override ZString Description
			{
				get { return "[ENG] Split duplicate HBL Container Mode Headers into their own strip."; }
			}

			public override void Execute()
			{
				var template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System, Enterprise.Core.Constants.Languages.English);
				var manipulator = new ConfigurableTemplateManipulator(template);

				manipulator.SplitSectionAndConfigItems(
					"Header with Bill Number / Available and Storage Dates",
					3,
					"Document Title with HBL Container Mode",
					"Recipient with Bill Number + Available + Storage Dates");

				manipulator.SplitReplaceAndRemove(
					"Header with Bill Number / Brand Name",
					1,
					"Header with Bill Number / Brand Name (1)",
					"Brand Name");

				manipulator.SplitReplaceAndRemove(
					"Header with Bill Number / Brand Name (1)",
					3,
					"Recipient with Bill Number",
					"Document Title with HBL Container Mode");
			}
		}

#if DEBUG
		internal
#endif
		class SplitDuplicateServiceDocumentTitlesEnglish : UpdateTemplateCommand
		{
			public SplitDuplicateServiceDocumentTitlesEnglish(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override ZString Description
			{
				get { return "[ENG] Split duplicate Service Document Titles into their own strip."; }
			}

			public override void Execute()
			{
				var template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System, Enterprise.Core.Constants.Languages.English);
				var manipulator = new ConfigurableTemplateManipulator(template);

				manipulator.SplitSectionAndConfigItems(
					"Service Header + Contractor Name and Address + Booked Date",
					3,
					"Service Document Title",
					"Recipient with Contractor Name + Address + Booked Date");

				manipulator.SplitReplaceAndRemove(
					"Service Header + Contractor Name and Address + Booked Date (Minimal)",
					3,
					"Recipient with Contractor Name",
					"Service Document Title");
			}
		}

#if DEBUG
		internal
#endif
		class SplitDuplicateShortModeDepartureArrivalHeadersGerman : UpdateTemplateCommand
		{
			public SplitDuplicateShortModeDepartureArrivalHeadersGerman(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override ZString Description
			{
				get { return "[GRM] Split duplicate Mode + Departure/Arrival headers (Short) into their own strip."; }
			}

			public override void Execute()
			{
				var template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System, Enterprise.Core.Constants.Languages.German);
				var manipulator = new ConfigurableTemplateManipulator(template);

				manipulator.SplitSectionAndConfigItems(
					"Mode + Departure/Arrival Header",
					3,
					"Document Title with Mode + Departure/Arrival (Short)",
					"Recipient with Job Number");

				manipulator.SplitReplaceAndRemove(
					"Mode + Departure/Arrival Header + Available Date + Storage Date",
					3,
					"Recipient with Available Date + Storage Date",
					"Document Title with Mode + Departure/Arrival (Short)");

				manipulator.SplitReplaceAndRemove(
					"Mode + Departure/Arrival Header with Container Number + Available Date + Storage Date",
					3,
					"Recipient with Container Number + Available Date + Storage Date",
					"Document Title with Mode + Departure/Arrival (Short)");

				manipulator.SplitReplaceAndRemove(
					"Mode + Departure/Arrival Header with Container Number + Available Date + Storage Date (Minimal)",
					3,
					"Recipient with Container Number",
					"Document Title with Mode + Departure/Arrival (Short)");
			}
		}

		#region Implementation

		void SaveToFile(StmTemplateBase template)
		{
			if (File.Exists(template.ExcelTemplateFullPath))
			{
				using (var excelInterface = new ExcelInterface())
				{
					using (var stream = template.GetExcelTemplate().GetAsTemplateStream())
					{
						excelInterface.LoadExcelFile(stream);
					}

					excelInterface.SaveToFile(template.ExcelTemplateFullPath);
				}
			}
		}

		#endregion
	}
}
#endif
