using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class CMRAirCargoStandAloneHouseMessageUserControl
	{
		void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.OrderedMessagesBoundGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// OrderedMessagesBoundGrid
			// 
			this.OrderedMessagesBoundGrid.BindTo = "FilteredChildBills.Messages";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)));
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_MessageNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_MessageNum)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_MessageTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_MessageType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_SystemCreateTimeUtc)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_SystemCreateTimeUtcInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_MessageSubTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_MessageSubType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_MessageSubTypeDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_MessageSubTypeDescription)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_InterchangeNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_InterchangeNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_DateTimeInterchangeSent)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_DateTimeInterchangeSentInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_UserInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_User)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_ReceiveTransmitInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_ReceiveTransmit)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_StatusInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).EM_Status)));
			// 
			// EM_MessageTextBoundTextBox
			// 
			this.EM_MessageTextBoundTextBox.BindTo = "FilteredChildBills.Messages.HumanReadableMessage";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).HumanReadableMessageInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).FilteredChildBills)))).Messages)))).HumanReadableMessage)));
			// 
			// CMRAirCargoStandAloneHouseMessageUserControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.Name = "CMRAirCargoStandAloneHouseMessageUserControl";
			((System.ComponentModel.ISupportInitialize)(this.OrderedMessagesBoundGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
