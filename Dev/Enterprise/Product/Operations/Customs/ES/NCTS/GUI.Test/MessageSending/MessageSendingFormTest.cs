using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;
using NctsHeaderMessageSendingObjectParent = Enterprise.Customs.ES.NCTS.Business.NctsHeaderMessageSendingObjectParent;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	public class MessageSendingFormTest : MessageSendingObjectFormTest
	{
		[RequiresSTA]
		public void TestGetBottomSectionUserControl()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			Factory.Save();
			CombineAssertions(() =>
			{
				using (var testForm = new MessageSendingForm(new NctsHeaderMessageSendingObjectParent(new NctsMessageSendingObject(header, GlbStaff.CurrentUser))))
				{
					testForm.Show();
					var splitContainer = testForm.FindSingleOrDefault<KSplitContainer>("WarningSplitContainer");
					AssertType<MessageSendingFormBottomSectionUserControl>("Use parent control when Phase4", splitContainer.Panel2.Controls[0]);
				}

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				Factory.Save();
				using (var testForm = new MessageSendingForm(new NctsHeaderMessageSendingObjectParent(new NctsMessageSendingObject(header, GlbStaff.CurrentUser))))
				{
					testForm.Show();
					var splitContainer = testForm.FindSingleOrDefault<KSplitContainer>("WarningSplitContainer");
					AssertType<Ncts5BottomSectionUserControl>("Use ES control when Phase5", splitContainer.Panel2.Controls[0]);
				}
			});
		}

		[RequiresSTA]
		public void TestTabStopWhenAllColumnsReadOnly()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			Factory.Save();
			CombineAssertions(() =>
			{
				using (var testForm = new MessageSendingForm(new NctsHeaderMessageSendingObjectParent(new NctsMessageSendingObject(header, GlbStaff.CurrentUser))))
				{
					testForm.Show();
					var messageSendingObjectsGrid = testForm.FindSingleOrDefault<ZGrid>("MessageSendingObjectsGrid");
					AssertEquals("For Phase4, when all columns are readonly MessageSendingObjectsGrid.TabStop is false", false, messageSendingObjectsGrid.TabStop);
				}

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				Factory.Save();
				using (var testForm = new MessageSendingForm(new NctsHeaderMessageSendingObjectParent(new NctsMessageSendingObject(header, GlbStaff.CurrentUser))))
				{
					testForm.Show();
					var messageSendingObjectsGrid = testForm.FindSingleOrDefault<ZGrid>("MessageSendingObjectsGrid");
					AssertEquals("For Phase5, when all columns are readonly MessageSendingObjectsGrid.TabStop is false", false, messageSendingObjectsGrid.TabStop);
				}
			});
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			Factory.Save();
			return new MessageSendingForm(new NctsHeaderMessageSendingObjectParent(new NctsMessageSendingObject(header, GlbStaff.CurrentUser)));
		}

		public override void TestBashingForm()
		{
			Assert(true);
		}

		[DeveloperOnlyTest]
		public override void TestMarkAsNeedingValidationIsNotCalledWhenFormLoads()
		{
			Assert(true);
		}
	}
}
