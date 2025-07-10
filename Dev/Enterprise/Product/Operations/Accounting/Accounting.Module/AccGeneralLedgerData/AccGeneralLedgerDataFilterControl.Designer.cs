namespace Enterprise.Accounting.Module
{
	public partial class AccGeneralLedgerDataFilterControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			this.RecentItemsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// grid
			//
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_GC_Company)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_GB_Branch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_GB_TaxBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_GE_Department)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_PostPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_AG_GLAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_OSDebitAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_OSCreditAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_LocalDebitAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_LocalCreditAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).OrganizationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_SystemLastEditTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_SystemLastEditUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).TransactionNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).TransactionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).JobHeaderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).ChargeCodePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLAccountDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).SubAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).HeaderDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).LineDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).PresentationJournalCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).Units)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData)(null)).GLD_JournalEntriesNumber)));

			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("6b7f29b1-710c-4906-adfe-bb5c13f7246a", "Company Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GLD_GC_Company";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("69c7ce14-7bc3-436f-a524-6142ef40ad1a", "Branch Code");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "GLD_GB_Branch";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("a0e7d942-6545-4cd4-bb30-7b49fa16b6ed", "Tax Branch Code");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "GLD_GB_TaxBranch";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("fd2164df-90c6-4570-8430-bd291d6d221a", "Department Code");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "GLD_GE_Department";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("e737d0e3-b9b9-47cc-9b59-dd1d94626fa1", "Post Date");
			zDateEditColumnStyleInfo1.ColumnName = "GLD_PostDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ShowGroupSeparators = false;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("6444a786-214f-4e61-9700-418a7d393d78", "Post Period");
			zCalcEditColumnStyleInfo1.ColumnName = "GLD_PostPeriod";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("a01212ba-55b6-4670-b397-72cc12b19c69", "GL Account");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "GLD_AG_GLAccount";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("f846e7ef-41c8-494f-ad6d-719cb8aafcd8", "OS Debit Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "GLD_OSDebitAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("0705f13f-19b1-4ef4-b3c4-e01bebb0365a", "OS Credit Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "GLD_OSCreditAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("e94ce3d2-844a-44b1-ba8f-120ba8eb4a6e", "Local Debit Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "GLD_LocalDebitAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("f76be95e-aba1-4983-b8c3-f7930514345b", "Local Credit Amount");
			zCalcEditColumnStyleInfo5.ColumnName = "GLD_LocalCreditAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("49b77a25-283f-4c26-bbd6-f2c61e54fa4b", "Organization Code");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OrganizationPK";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("db1129c2-d068-4514-b069-3ccb70d87a71", "Currency");
			zTextBoxColumnStyleInfo1.ColumnName = "GLD_Currency";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("fd1b86c8-4370-47bd-824e-d4f18a197888", "Exchange Rate");
			zCalcEditColumnStyleInfo6.ColumnName = "GLD_ExchangeRate";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("2f8106fe-48a0-404b-8f86-9ab655e94752", "Transaction Type");
			zTextBoxColumnStyleInfo2.ColumnName = "TransactionType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("4B5098AB-C615-4D0D-A979-986B6E6000F9", "Journal Entries Type");
			zTextBoxColumnStyleInfo13.ColumnName = "GLD_Type";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("7b48a363-a1c1-4b2f-8f5d-058128fe3387", "Created Time");
			zDateEditColumnStyleInfo2.ColumnName = "GLD_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("7518f9a7-210d-4671-9db4-bc42fe5f2cb5", "Creating User");
			zTextBoxColumnStyleInfo3.ColumnName = "GLD_SystemCreateUser";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("576beb89-28d8-4761-b855-b1949a495f8c", "Last Edit Time");
			zDateEditColumnStyleInfo3.ColumnName = "GLD_SystemLastEditTimeUtc";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("6756d535-a780-4cec-ad4f-ccc04d80fe56", "Last Edit User");
			zTextBoxColumnStyleInfo4.ColumnName = "GLD_SystemLastEditUser";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "Ledger";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("8BDCC087-9194-45B9-9EAF-BC3319ED2B0C", "Ledger");
			zTextBoxColumnStyleInfo5.ColumnName = "TransactionNumber";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("BE24939C-74EF-43B6-BADC-E4FF0DADCE96", "Transaction Number");
			zDateEditColumnStyleInfo4.ColumnName = "DueDate";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("D359702C-92F8-4C34-88B4-63896237FFAB", "Due Date");
			zDateEditColumnStyleInfo5.ColumnName = "TransactionDate";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("69AAA483-D1E8-432D-9A88-089265B54801", "Transaction Date");
			zGuidFindBoxColumnStyleInfo6.ColumnName = "JobHeaderPK";
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("9622191B-6B96-408B-83AC-208C0579CACE", "Job Number");
			zGuidFindBoxColumnStyleInfo7.ColumnName = "ChargeCodePK";
			zGuidFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("C158EBF6-1151-43F7-A368-68246366A893", "Charge Code");
			zTextBoxColumnStyleInfo7.ColumnName = "GLAccountDesc";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("46D17200-2BA9-42E7-AAD9-A23C6D423FCD", "Account Name");
			zTextBoxColumnStyleInfo8.ColumnName = "SubAccount";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("36289D6D-0797-4D47-BCB8-77EC1E58451B", "Sub Account");
			zTextBoxColumnStyleInfo9.ColumnName = "HeaderDescription";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("2E9D582B-C7C5-493B-B851-366B789AD68A", "Header Description");
			zTextBoxColumnStyleInfo10.ColumnName = "LineDescription";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("3196D760-A39C-4812-8F4B-CAF69EC5E26F", "Line Description");
			zTextBoxColumnStyleInfo11.ColumnName = "PresentationJournalCategory";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("39C837B3-DBDA-4D1F-9199-1D266F732826", "Presentation Journal Category");
			zTextBoxColumnStyleInfo12.ColumnName = "Units";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("6E387431-4C1E-4F32-AEE3-9BF5E2A7C29D", "Units");
			zTextBoxColumnStyleInfo14.ColumnName = "GLD_JournalEntriesNumber";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("e7b8f8e2-3c4a-4d2b-9f8e-1a2b3c4d5e6f", "Journal Entries Number");

			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);

			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 126, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1371, 433, true);
			//
			// AddStripButton
			//
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(661, 25, true);
			//
			// FilterStripsPanel
			//
			this.FilterStripsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.GeneralLedgerData.Business.AccGeneralLedgerData);
			//
			// AccGeneralLedgerDataFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "AccGeneralLedgerDataFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1371, 559, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			this.RecentItemsPanel.ResumeLayout(false);
			this.RecentItemsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
