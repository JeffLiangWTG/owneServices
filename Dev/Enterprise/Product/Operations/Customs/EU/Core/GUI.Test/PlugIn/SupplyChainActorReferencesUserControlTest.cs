using System.Collections;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	class SupplyChainActorReferencesUserControlTest : TestCaseWithFactory
	{
		public void TestGroupBoxCaption()
		{
			var supplyChainActorReferencesGroupBox = control.FindSingle<ZGroupBox>("SupplyChainActorReferencesGroupBox");
			AssertEquals("GroupBox caption", "Additional Supply Chain Actor", supplyChainActorReferencesGroupBox.CaptionResourceString.Caption);
		}

		public void TestIdentificationCharacterCasing()
		{
			var supplyChainActorReferencesGrid = control.FindSingle<ZGrid>("SupplyChainActorReferencesGrid");
			AssertEquals("Identification Character Casing", CharacterCasing.Upper, supplyChainActorReferencesGrid.GetColumnStyle(AutoCusReference.Schema.CFR_Reference).CharacterCasing);
		}

		public void TestAvailableColumns()
		{
			var supplyChainActorReferencesGrid = control.FindSingle<ZGrid>("SupplyChainActorReferencesGrid");
			AssertSequencesEqual("Columns",
				new[] { AutoCusReference.Schema.CFR_Code, AutoCusReference.Schema.CFR_Reference, CommonCusReference.Schema.OwnerOrgPK },
				supplyChainActorReferencesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray());
		}

		public void TestColumnWidths()
		{
			var supplyChainActorReferencesGrid = control.FindSingle<ZGrid>("SupplyChainActorReferencesGrid");
			CombineAssertions(() =>
			{
				AssertEquals("CFR_Code", 40, supplyChainActorReferencesGrid.GetColumnStyle(AutoCusReference.Schema.CFR_Code).Width);
				AssertEquals("CFR_Reference", 130, supplyChainActorReferencesGrid.GetColumnStyle(AutoCusReference.Schema.CFR_Reference).Width);
				AssertEquals("OwnerOrgPK", 87, supplyChainActorReferencesGrid.GetColumnStyle(CommonCusReference.Schema.OwnerOrgPK).Width);
			});
		}

		public void TestReferenceColumnCaption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(jobDeclaration))
			{
				control.SetDataBinding(jobDeclaration, "FilteredInvoiceLines.CusSupplyChainActorReferences");
				form.Controls.Add(control);
				form.Show();

				var supplyChainActorReferencesGrid = control.FindSingle<ZGrid>("SupplyChainActorReferencesGrid");
				AssertEquals("CFR_Reference", "Reference", supplyChainActorReferencesGrid.GetColumnCaption(AutoCusReference.Schema.CFR_Reference));
			}
		}

		public void TestReferenceColumnCaption_HasOverwrittenCaption()
		{
			var mockedCusAuthorisationHeaderProviderObjectHeadle = new Hashtable { { Core.Constants.CountryCodes.Latvia, new CusSupplyChainActorReferenceProviderObjectHandleForTest() } };
			using (ObjectFactory.Substitute("CusSupplyChainActorReferenceProviders", mockedCusAuthorisationHeaderProviderObjectHeadle))
			{
				var jobDeclaration = Factory.New<JobDeclaration>();
				using (var form = new ZForm(jobDeclaration))
				{
					control.SetDataBinding(jobDeclaration, "FilteredInvoiceLines.CusSupplyChainActorReferences");
					form.Controls.Add(control);
					form.Show();

					var supplyChainActorReferencesGrid = control.FindSingle<ZGrid>("SupplyChainActorReferencesGrid");
					AssertEquals("CFR_Reference", "New Reference", supplyChainActorReferencesGrid.GetColumnCaption(AutoCusReference.Schema.CFR_Reference));
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new SupplyChainActorReferencesUserControl();
		}
		SupplyChainActorReferencesUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}

	class CusSupplyChainActorReferenceProviderObjectHandleForTest : ObjectHandle
	{
		public override object GetObject(params object[] arguments) => new CusSupplyChainActorReferenceProviderForTest(arguments[0].ToString());
	}

	class CusSupplyChainActorReferenceProviderForTest : CusSupplyChainActorReferenceProvider
	{
		public CusSupplyChainActorReferenceProviderForTest(ZString dataGroupingCode) : base(dataGroupingCode)
		{
		}

		protected override ZString OverwrittenReferenceColumnCaptionCore => "New Reference";
	}
}
