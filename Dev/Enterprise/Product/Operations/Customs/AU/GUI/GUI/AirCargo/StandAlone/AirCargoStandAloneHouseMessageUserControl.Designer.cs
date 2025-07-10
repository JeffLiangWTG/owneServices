using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCargoStandAloneHouseMessageUserControl
	{
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.OrderedMessagesBoundGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusMAWB);
			// 
			// OrderedMessagesBoundGrid
			// 
			this.OrderedMessagesBoundGrid.BindTo = "CurrentHouseBills.Messages";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)));
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_MessageNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_MessageNum)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_MessageTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_MessageType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_SystemCreateTimeUtc)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_SystemCreateTimeUtcInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_MessageSubTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_MessageSubType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_MessageSubTypeDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_MessageSubTypeDescription)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_InterchangeNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_InterchangeNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_DateTimeInterchangeSent)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_DateTimeInterchangeSentInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_UserInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).EM_User)));
			// 
			// EM_MessageTextBoundTextBox
			// 
			this.EM_MessageTextBoundTextBox.BindTo = "CurrentHouseBills.Messages.HumanReadableMessage";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).HumanReadableMessageInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((EDIMessage)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusHAWB)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(null)).CurrentHouseBills)))).Messages)))).HumanReadableMessage)));
			// 
			// AirCargoStandAloneHouseMessageUserControl
			// 
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusMAWB";
			this.Name = "AirCargoStandAloneHouseMessageUserControl";
			((System.ComponentModel.ISupportInitialize)(this.OrderedMessagesBoundGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
