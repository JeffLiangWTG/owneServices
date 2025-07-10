using System.IO;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.GUI.Testing
{
	[TestedType(typeof(DataExportForm))]
	public class DataExportFormTest : ZFormBasherTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Testing")]
		public void TestOutputTextBoxIsReadOnly()
		{
			using (TestDataExportForm form = new TestDataExportForm())
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("OutputTextbox.ReadOnly", true, form.OutputTextbox.ReadOnly);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Testing")]
		[RequiresSTA]
		public void TestINotificationSubscriber_Notify()
		{
			using (TestDataExportForm form = new TestDataExportForm())
			{
				form.Show();
				Application.DoEvents();
				form.Notify(new ErrorNotification(ErrorType.Error, "MyError"));
				form.Notify(new InfoNotification(null));
				form.Notify(new InfoNotification("MyInfo"));
				form.Notify(new NewlineNotification());
				AssertEquals("Error: MyError\r\nMyInfo\r\n\r\n", form.OutputTextbox.Text);
			}
		}

		public void TestOutput()
		{
			TestFlatFileDataExporterThrowsException exporter = new TestFlatFileDataExporterThrowsException(Factory);
			FilteredBusinessObjectReader reader = new FilteredBusinessObjectReader(new ZQuery(), typeof(OrgHeader));
			using (TestDataExportForm form = new TestDataExportForm(exporter, reader))
			{
				form.Show();
				form.Export();
				try
				{
					Assert("exception should be thown on export. output text box will say unsuccessful", form.OutputTextbox.Text.IndexOf("unsuccessful") != -1);
				}
				finally
				{
					if (File.Exists(exporter.ExportedFileForTesting))
					{
						File.Delete(exporter.ExportedFileForTesting);
					}
				}
			}
		}

		public void TestExportOK_Reader()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.Equal, "ABABEU");

			FilteredBusinessObjectReader bizObjReader = new FilteredBusinessObjectReader(filter, typeof(OrgHeader));

			TestFlatFileDataExporterWithOrgDataAdapter exporter = new TestFlatFileDataExporterWithOrgDataAdapter(Factory);

			using (TestDataExportForm form = new TestDataExportForm(exporter, bizObjReader))
			{
				form.Show();

				exporter.SetExportOK(true);
				form.Export();
				try
				{
					Assert("Should Export succesfully when IsExportOK set true. output text box will say complete", form.OutputTextbox.Text.IndexOf("complete") != -1);
				}
				finally
				{
					DeleteIfExists(exporter.ExportedFileForTesting);
				}

				exporter.SetExportOK(false);
				form.Export();
				try
				{
					Assert("Should not Export when IsExportOK set false. output text box will say unsuccessful", form.OutputTextbox.Text.IndexOf("unsuccessful") != -1);
				}
				finally
				{
					DeleteIfExists(exporter.ExportedFileForTesting);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			FlatFileDataExporterForTesting exporter = new FlatFileDataExporterForTesting(Factory);
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "AB");
			OrgHeaderCollection collection = new OrgHeaderCollection(Factory, filter);
			CollectionWrapperBusinessObjectReader reader = new CollectionWrapperBusinessObjectReader(collection);
			return NewExportForm(exporter, reader);
		}

		protected virtual DataExportForm NewExportForm(FlatFileDataExporterForTesting exporter, CollectionWrapperBusinessObjectReader reader)
		{
			return new DataExportForm(exporter, reader);
		}

		protected override bool AllowFormSizeFixed => true;

		class TestDataExportForm : DataExportForm
		{
			public TestDataExportForm()
				: this(new TestFlatFileDataExporter(new BusinessObjectFactory()), new FilteredBusinessObjectReader(new ZQuery(), typeof(OrgHeader)))
			{
			}

			public TestDataExportForm(FlatFileDataExporter exporter, BusinessObjectReader bizObjReader)
				: base(exporter, bizObjReader)
			{
			}

			public new ZTextBox OutputTextbox
			{
				get { return base.OutputTextbox; }
			}
		}

		class TestFlatFileDataExporter : FlatFileDataExporter
		{
			public TestFlatFileDataExporter(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override IFlatFileConverter CreateConverter(INotifications notifications)
			{
				return null;
			}

			protected override IValueObjectDataAdapter DataAdapter
			{
				get { return null; }
			}

			public override ZString EnglishDescription
			{
				get { return new ZString(); }
			}

			protected override IFlatFileFormat FlatFileFormat
			{
				get { return null; }
			}
		}

		class TestFlatFileDataExporterWithOrgDataAdapter : FlatFileDataExporter
		{
			public TestFlatFileDataExporterWithOrgDataAdapter(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public void SetExportOK(bool value)
			{
				SetIsExportOK(value);
			}

			protected override IFlatFileConverter CreateConverter(INotifications notifications)
			{
				return new ConverterForTest(notifications, Factory);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
			protected override void PopulateExportInstructions(ExportInstructions instructions, IFlatFileConverter converter)
			{
				base.PopulateExportInstructions(instructions, converter);
				instructions.SpecifiedFilename = BaseSourcePath + @"Test";
				instructions.MethodOfExport = ExportType.File;
			}

			protected override IValueObjectDataAdapter DataAdapter
			{
				get { return new OrganisationValueObjectDataAdapter(); }
			}

			public override ZString EnglishDescription
			{
				get { return new ZString(); }
			}

			protected override IFlatFileFormat FlatFileFormat
			{
				get { return new FlatFileFormatForDataTest(); }
			}
		}

		class FlatFileFormatForDataTest : FlatFileFormat
		{
			public override FlatFileDataRow ConvertToRow(ZString rawRow)
			{
				return new FlatFileDataRow(1);
			}

			public override ZString ConvertToLine(FlatFileDataRow row)
			{
				return new ZString();
			}

			public override FileExtensionType FileExtensionForExport
			{
				get { return FileExtensionType.Txt; }
			}

			public override FileExtensionType FileExtensionForImport
			{
				get { return FileExtensionType.Txt; }
			}
		}

		class ConverterForTest : FlatFileConverter
		{
			public ConverterForTest(INotifications notification, BusinessObjectFactory factory)
				: base(notification, factory)
			{
			}
		}

		class TestFlatFileDataExporterThrowsException : TestFlatFileDataExporter
		{
			public TestFlatFileDataExporterThrowsException(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override IFlatFileConverter CreateConverter(INotifications notifications)
			{
				throw new IOException();
			}
		}
	}
}
