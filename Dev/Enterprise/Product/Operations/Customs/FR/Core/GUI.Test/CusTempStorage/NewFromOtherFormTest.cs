using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	[TestedType(typeof(NewFromOtherForm))]
	class NewFromOtherFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var helper = new TemporaryStorageWrapperFromParentHelper(Factory);
			return new NewFromOtherForm(helper);
		}

		void AssertCreateNewFromOtherForm(TemporaryStorageWrapperFromParentHelper helper, ZGuid pk, ZString type, ZString prefix)
		{
			using (var form = new NewFromOtherFormForTest(helper))
			{
				form.Show();
				form.OkButton_Click(form.okButton, EventArgs.Empty);

				AssertType<CusTempStorageForm>(ZFormModaliser.LastFormShownDialogForTest);

				var pivotQuery = new ZQuery(GenPivotSchema.XX_Relation2ID, pk);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, prefix);
				var genPivot = Factory.LoadTop1<GenPivot>(pivotQuery);

				var header = genPivot.Relation1Object as CusTempStorageJobHeader;
				AssertNotNull(header);
				AssertEquals(type, genPivot.XX_RelationType);
			}
		}

		public void TestCreateNewFromShipment()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = orgHeader.PK;
			var helper = new TemporaryStorageWrapperFromParentHelper(Factory);
			helper.ShipmentPK = shipment.PK;
			Factory.Save();

			AssertCreateNewFromOtherForm(helper, shipment.PK, Core.Constants.GenPivotTypes.CusStorageHeaderShipment, JobShipmentSchema.Constants.Prefix);
		}

		public void TestCreateNewFromDeclaration()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<Business.Declaration.JobDeclaration>();
			declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
			var helper = new TemporaryStorageWrapperFromParentHelper(Factory);
			helper.DeclarationPK = declaration.PK;
			Factory.Save();

			AssertCreateNewFromOtherForm(helper, declaration.PK, Core.Constants.GenPivotTypes.CusStorageHeaderDeclaration, JobDeclarationSchema.Constants.Prefix);
		}

		public void TestCreateNewFromNctsHeader()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			nctsHeader.DestinationTrader.OrganisationPK = orgHeader.PK;
			var helper = new TemporaryStorageWrapperFromParentHelper(Factory);
			helper.DeltaTPK = nctsHeader.PK;
			Factory.Save();

			AssertCreateNewFromOtherForm(helper, nctsHeader.PK, Core.Constants.GenPivotTypes.CusStorageHeaderNctsHeader, CusInBondHeaderSchema.Constants.Prefix);
		}

		class NewFromOtherFormForTest : NewFromOtherForm
		{
			public NewFromOtherFormForTest(TemporaryStorageWrapperFromParentHelper helper) : base(helper)
			{
			}

			public new ZButton okButton => base.okButton;

			public new void OkButton_Click(object sender, EventArgs e) => base.OkButton_Click(sender, e);
		}
	}
}
