using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoHouseUnderbondControl
	{
		private void InitializeComponent()
		{
			this.ContainerUnderbondMovementGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CNContainerNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContainerNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel21 = new Enterprise.ZArchitecture.ZLabel();
			this.CN_UnderbondVoyageBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zCheckBox3 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCheckBox2 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CN_UnderbondStatusBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zAddressControl3 = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.zAddressControl2 = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CN_MoveUnderbondFromTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel42 = new Enterprise.ZArchitecture.ZLabel();
			this.CN_MoveUnderbondToTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel53 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel54 = new Enterprise.ZArchitecture.ZLabel();
			this.UnderbondHouseBillGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.HouseBill = new Enterprise.ZArchitecture.ZLabel();
			this.CA_OA_UnderbondFromBoundAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CA_OA_UnderbondToBoundAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.zLabel41 = new Enterprise.ZArchitecture.ZLabel();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CS_MoveUnderbondToTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_MoveUnderbondFromTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel49 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel50 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_UnderbondStatusBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContainerUnderbondMovementGroupBox.SuspendLayout();
			this.UnderbondHouseBillGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// ContainerUnderbondMovementGroupBox
			// 
			this.ContainerUnderbondMovementGroupBox.Controls.Add(this.CNContainerNumberBoundTextBox);
			this.ContainerUnderbondMovementGroupBox.Controls.Add(this.ContainerNumberLabel);
			this.ContainerUnderbondMovementGroupBox.Controls.Add(this.zLabel21);
			this.ContainerUnderbondMovementGroupBox.Controls.Add(this.CN_UnderbondVoyageBoundTextBox);
			this.ContainerUnderbondMovementGroupBox.Controls.Add(this.zCheckBox3);
			this.ContainerUnderbondMovementGroupBox.Controls.Add(this.zCheckBox2);
			this.ContainerUnderbondMovementGroupBox.Controls.Add(this.CN_UnderbondStatusBoundTextBox);
			this.ContainerUnderbondMovementGroupBox.Controls.Add(this.zAddressControl3);
			this.ContainerUnderbondMovementGroupBox.Controls.Add(this.zAddressControl2);
			this.ContainerUnderbondMovementGroupBox.Controls.Add(this.CN_MoveUnderbondFromTextBox);
			this.ContainerUnderbondMovementGroupBox.Controls.Add(this.zLabel42);
			this.ContainerUnderbondMovementGroupBox.Controls.Add(this.CN_MoveUnderbondToTextBox);
			this.ContainerUnderbondMovementGroupBox.Controls.Add(this.zLabel53);
			this.ContainerUnderbondMovementGroupBox.Controls.Add(this.zLabel54);
			this.ContainerUnderbondMovementGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ContainerUnderbondMovementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 0, true);
			this.ContainerUnderbondMovementGroupBox.Name = "ContainerUnderbondMovementGroupBox";
			this.ContainerUnderbondMovementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 168, true);
			this.ContainerUnderbondMovementGroupBox.TabIndex = 10;
			this.ContainerUnderbondMovementGroupBox.TabStop = false;
			this.ContainerUnderbondMovementGroupBox.Text = "Container Underbond Movement";
			// 
			// CNContainerNumberBoundTextBox
			// 
			this.CNContainerNumberBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.CNContainerNumberBoundTextBox.BindTo = "Pivot.Container+CN_ContainerNumberReadOnly";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_ContainerNumberReadOnlyInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_ContainerNumberReadOnly)));
			this.CNContainerNumberBoundTextBox.ReadOnly = true;
			this.CNContainerNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 40, true);
			this.CNContainerNumberBoundTextBox.Name = "CNContainerNumberBoundTextBox";
			this.CNContainerNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.CNContainerNumberBoundTextBox.TabIndex = 106;
			this.CNContainerNumberBoundTextBox.Text = "";
			this.ContainerControlToolTip.SetToolTip(this.CNContainerNumberBoundTextBox, "Sea Cargo Underbond Status");
			// 
			// ContainerNumberLabel
			// 
			this.ContainerNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.ContainerNumberLabel.Name = "ContainerNumberLabel";
			this.ContainerNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 23, true);
			this.ContainerNumberLabel.TabIndex = 105;
			this.ContainerNumberLabel.Text = "Container Number:";
			// 
			// zLabel21
			// 
			this.zLabel21.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 88, true);
			this.zLabel21.Name = "zLabel21";
			this.zLabel21.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel21.TabIndex = 104;
			this.zLabel21.Text = "Voyage:";
			// 
			// CN_UnderbondVoyageBoundTextBox
			// 
			this.CN_UnderbondVoyageBoundTextBox.BindTo = "Pivot.Container.CN_UnderbondVoyage";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_UnderbondVoyageInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_UnderbondVoyage)));
			this.CN_UnderbondVoyageBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 112, true);
			this.CN_UnderbondVoyageBoundTextBox.Name = "CN_UnderbondVoyageBoundTextBox";
			this.CN_UnderbondVoyageBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.CN_UnderbondVoyageBoundTextBox.TabIndex = 6;
			this.CN_UnderbondVoyageBoundTextBox.Text = "";
			this.ContainerControlToolTip.SetToolTip(this.CN_UnderbondVoyageBoundTextBox, "Underbond from Location ID");
			// 
			// zCheckBox3
			// 
			this.zCheckBox3.BindTo = "Pivot.Container.CN_UnderbondBySea";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_UnderbondBySea)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_UnderbondBySeaInfo)));
			this.zCheckBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 64, true);
			this.zCheckBox3.Name = "zCheckBox3";
			this.zCheckBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 16, true);
			this.zCheckBox3.TabIndex = 5;
			this.zCheckBox3.Text = "By Sea";
			this.ContainerControlToolTip.SetToolTip(this.zCheckBox3, "Do the goods require a fumigation certificate?");
			// 
			// zCheckBox2
			// 
			this.zCheckBox2.BindTo = "Pivot.Container.CN_TimeupUnderbondMove";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_TimeupUnderbondMove)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_TimeupUnderbondMoveInfo)));
			this.zCheckBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 64, true);
			this.zCheckBox2.Name = "zCheckBox2";
			this.zCheckBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.zCheckBox2.TabIndex = 4;
			this.zCheckBox2.Text = "Time Up";
			this.ContainerControlToolTip.SetToolTip(this.zCheckBox2, "Do the goods require a fumigation certificate?");
			// 
			// CN_UnderbondStatusBoundTextBox
			// 
			this.CN_UnderbondStatusBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.CN_UnderbondStatusBoundTextBox.BindTo = "Pivot.Container.CN_UnderbondStatus";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_UnderbondStatusInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_UnderbondStatus)));
			this.CN_UnderbondStatusBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 16, true);
			this.CN_UnderbondStatusBoundTextBox.Name = "CN_UnderbondStatusBoundTextBox";
			this.CN_UnderbondStatusBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
			this.CN_UnderbondStatusBoundTextBox.TabIndex = 0;
			this.CN_UnderbondStatusBoundTextBox.Text = "";
			this.ContainerControlToolTip.SetToolTip(this.CN_UnderbondStatusBoundTextBox, "Sea Cargo Underbond Status");
			// 
			// zAddressControl3
			// 
			this.zAddressControl3.BindToAddress = "Pivot.Container.CN_OA_UnderbondTo";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_OA_UnderbondTo_ZAddress)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_OA_UnderbondTo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_OA_UnderbondToInfo)));
			this.zAddressControl3.BindToOrgList = "Pivot.UnderbondToCollection";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.BusinessObjectCollection)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).UnderbondToCollection)));
			this.zAddressControl3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 136, true);
			this.zAddressControl3.Name = "zAddressControl3";
			this.zAddressControl3.PopupCaption = "";
			this.zAddressControl3.ShowAddress = false;
			this.zAddressControl3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 21, true);
			this.zAddressControl3.TabIndex = 3;
			// 
			// zAddressControl2
			// 
			this.zAddressControl2.BindToAddress = "Pivot.Container.CN_OA_UnderbondFrom";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_OA_UnderbondFrom_ZAddress)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_OA_UnderbondFrom)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_OA_UnderbondFromInfo)));
			this.zAddressControl2.BindToOrgList = "Pivot.UnderbondFromCollection";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.BusinessObjectCollection)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).UnderbondFromCollection)));
			this.zAddressControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 88, true);
			this.zAddressControl2.Name = "zAddressControl2";
			this.zAddressControl2.PopupCaption = "";
			this.zAddressControl2.ShowAddress = false;
			this.zAddressControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 21, true);
			this.zAddressControl2.TabIndex = 1;
			// 
			// CN_MoveUnderbondFromTextBox
			// 
			this.CN_MoveUnderbondFromTextBox.BindTo = "Pivot.Container.CN_MoveUnderbondFrom";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_MoveUnderbondFromInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_MoveUnderbondFrom)));
			this.CN_MoveUnderbondFromTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 64, true);
			this.CN_MoveUnderbondFromTextBox.Name = "CN_MoveUnderbondFromTextBox";
			this.CN_MoveUnderbondFromTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.CN_MoveUnderbondFromTextBox.TabIndex = 0;
			this.CN_MoveUnderbondFromTextBox.Text = "";
			this.ContainerControlToolTip.SetToolTip(this.CN_MoveUnderbondFromTextBox, "Underbond from Location ID");
			// 
			// zLabel42
			// 
			this.zLabel42.IsFontBold = true;
			this.zLabel42.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.zLabel42.Name = "zLabel42";
			this.zLabel42.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 23, true);
			this.zLabel42.TabIndex = 102;
			this.zLabel42.Text = "Underbond Status:";
			// 
			// CN_MoveUnderbondToTextBox
			// 
			this.CN_MoveUnderbondToTextBox.BindTo = "Pivot.Container.CN_MoveUnderbondTo";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_MoveUnderbondToInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAPivot)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Pivot)))).Container.CN_MoveUnderbondTo)));
			this.CN_MoveUnderbondToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 112, true);
			this.CN_MoveUnderbondToTextBox.Name = "CN_MoveUnderbondToTextBox";
			this.CN_MoveUnderbondToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.CN_MoveUnderbondToTextBox.TabIndex = 2;
			this.CN_MoveUnderbondToTextBox.Text = "";
			this.ContainerControlToolTip.SetToolTip(this.CN_MoveUnderbondToTextBox, "Underbond to location ID");
			// 
			// zLabel53
			// 
			this.zLabel53.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64, true);
			this.zLabel53.Name = "zLabel53";
			this.zLabel53.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 23, true);
			this.zLabel53.TabIndex = 100;
			this.zLabel53.Text = "From:";
			// 
			// zLabel54
			// 
			this.zLabel54.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 112, true);
			this.zLabel54.Name = "zLabel54";
			this.zLabel54.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 23, true);
			this.zLabel54.TabIndex = 99;
			this.zLabel54.Text = "To:";
			// 
			// UnderbondHouseBillGroupBox
			// 
			this.UnderbondHouseBillGroupBox.Controls.Add(this.zTextBox1);
			this.UnderbondHouseBillGroupBox.Controls.Add(this.HouseBill);
			this.UnderbondHouseBillGroupBox.Controls.Add(this.CA_OA_UnderbondFromBoundAddressControl);
			this.UnderbondHouseBillGroupBox.Controls.Add(this.CA_OA_UnderbondToBoundAddressControl);
			this.UnderbondHouseBillGroupBox.Controls.Add(this.zLabel41);
			this.UnderbondHouseBillGroupBox.Controls.Add(this.zCheckBox1);
			this.UnderbondHouseBillGroupBox.Controls.Add(this.CS_MoveUnderbondToTextBox);
			this.UnderbondHouseBillGroupBox.Controls.Add(this.CA_MoveUnderbondFromTextBox);
			this.UnderbondHouseBillGroupBox.Controls.Add(this.zLabel49);
			this.UnderbondHouseBillGroupBox.Controls.Add(this.zLabel50);
			this.UnderbondHouseBillGroupBox.Controls.Add(this.CA_UnderbondStatusBoundTextBox);
			this.UnderbondHouseBillGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UnderbondHouseBillGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnderbondHouseBillGroupBox.Name = "UnderbondHouseBillGroupBox";
			this.UnderbondHouseBillGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 168, true);
			this.UnderbondHouseBillGroupBox.TabIndex = 9;
			this.UnderbondHouseBillGroupBox.TabStop = false;
			this.UnderbondHouseBillGroupBox.Text = "House Bill Underbond Movement";
			// 
			// zTextBox1
			// 
			this.zTextBox1.BackColor = System.Drawing.SystemColors.Control;
			this.zTextBox1.BindTo = "CA_HouseBillReadOnly";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_HouseBillReadOnlyInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_HouseBillReadOnly)));
			this.zTextBox1.ReadOnly = true;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 40, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.zTextBox1.TabIndex = 103;
			this.zTextBox1.Text = "";
			this.ContainerControlToolTip.SetToolTip(this.zTextBox1, "Sea Cargo Underbond Status");
			// 
			// HouseBill
			// 
			this.HouseBill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.HouseBill.Name = "HouseBill";
			this.HouseBill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.HouseBill.TabIndex = 102;
			this.HouseBill.Text = "House Bill:";
			// 
			// CA_OA_UnderbondFromBoundAddressControl
			// 
			this.CA_OA_UnderbondFromBoundAddressControl.BindToAddress = "CA_OA_UnderbondFrom";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_OA_UnderbondFrom_ZAddress)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_OA_UnderbondFrom)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_OA_UnderbondFromInfo)));
			this.CA_OA_UnderbondFromBoundAddressControl.BindToOrgList = "UnderbondFromCollection";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.BusinessObjectCollection)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).UnderbondFromCollection)));
			this.CA_OA_UnderbondFromBoundAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 88, true);
			this.CA_OA_UnderbondFromBoundAddressControl.Name = "CA_OA_UnderbondFromBoundAddressControl";
			this.CA_OA_UnderbondFromBoundAddressControl.PopupCaption = "";
			this.CA_OA_UnderbondFromBoundAddressControl.ShowAddress = false;
			this.CA_OA_UnderbondFromBoundAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 21, true);
			this.CA_OA_UnderbondFromBoundAddressControl.TabIndex = 1;
			this.ContainerControlToolTip.SetToolTip(this.CA_OA_UnderbondFromBoundAddressControl, "Underbond from Organisation Controlled Location Address");
			// 
			// CA_OA_UnderbondToBoundAddressControl
			// 
			this.CA_OA_UnderbondToBoundAddressControl.BindToAddress = "CA_OA_UnderbondTo";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_OA_UnderbondTo_ZAddress)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_OA_UnderbondTo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_OA_UnderbondToInfo)));
			this.CA_OA_UnderbondToBoundAddressControl.BindToOrgList = "UnderbondToCollection";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.BusinessObjectCollection)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).UnderbondToCollection)));
			this.CA_OA_UnderbondToBoundAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 136, true);
			this.CA_OA_UnderbondToBoundAddressControl.Name = "CA_OA_UnderbondToBoundAddressControl";
			this.CA_OA_UnderbondToBoundAddressControl.PopupCaption = "";
			this.CA_OA_UnderbondToBoundAddressControl.ShowAddress = false;
			this.CA_OA_UnderbondToBoundAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 21, true);
			this.CA_OA_UnderbondToBoundAddressControl.TabIndex = 3;
			this.ContainerControlToolTip.SetToolTip(this.CA_OA_UnderbondToBoundAddressControl, "Underbond to Organisation Controlled Location Address");
			// 
			// zLabel41
			// 
			this.zLabel41.IsFontBold = true;
			this.zLabel41.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.zLabel41.Name = "zLabel41";
			this.zLabel41.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 23, true);
			this.zLabel41.TabIndex = 101;
			this.zLabel41.Text = "Underbond Status:";
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.BindTo = "CA_TimeupUnderbondMove";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_TimeupUnderbondMove)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_TimeupUnderbondMoveInfo)));
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 64, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.zCheckBox1.TabIndex = 4;
			this.zCheckBox1.Text = "Time Up";
			this.ContainerControlToolTip.SetToolTip(this.zCheckBox1, "Do the goods require a fumigation certificate?");
			// 
			// CS_MoveUnderbondToTextBox
			// 
			this.CS_MoveUnderbondToTextBox.BindTo = "CA_MoveUnderbondTo";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_MoveUnderbondToInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_MoveUnderbondTo)));
			this.CS_MoveUnderbondToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 112, true);
			this.CS_MoveUnderbondToTextBox.Name = "CS_MoveUnderbondToTextBox";
			this.CS_MoveUnderbondToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.CS_MoveUnderbondToTextBox.TabIndex = 2;
			this.CS_MoveUnderbondToTextBox.Text = "";
			this.ContainerControlToolTip.SetToolTip(this.CS_MoveUnderbondToTextBox, "Underbond to Controlled Location ID");
			// 
			// CA_MoveUnderbondFromTextBox
			// 
			this.CA_MoveUnderbondFromTextBox.BindTo = "CA_MoveUnderbondFrom";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_MoveUnderbondFromInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_MoveUnderbondFrom)));
			this.CA_MoveUnderbondFromTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 64, true);
			this.CA_MoveUnderbondFromTextBox.Name = "CA_MoveUnderbondFromTextBox";
			this.CA_MoveUnderbondFromTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.CA_MoveUnderbondFromTextBox.TabIndex = 0;
			this.CA_MoveUnderbondFromTextBox.Text = "";
			this.ContainerControlToolTip.SetToolTip(this.CA_MoveUnderbondFromTextBox, "Underbond from Controlled Location ID");
			// 
			// zLabel49
			// 
			this.zLabel49.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64, true);
			this.zLabel49.Name = "zLabel49";
			this.zLabel49.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 23, true);
			this.zLabel49.TabIndex = 100;
			this.zLabel49.Text = "From:";
			// 
			// zLabel50
			// 
			this.zLabel50.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 112, true);
			this.zLabel50.Name = "zLabel50";
			this.zLabel50.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 23, true);
			this.zLabel50.TabIndex = 99;
			this.zLabel50.Text = "To:";
			// 
			// CA_UnderbondStatusBoundTextBox
			// 
			this.CA_UnderbondStatusBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.CA_UnderbondStatusBoundTextBox.BindTo = "CA_UnderbondStatus";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_UnderbondStatusInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_UnderbondStatus)));
			this.CA_UnderbondStatusBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 16, true);
			this.CA_UnderbondStatusBoundTextBox.Name = "CA_UnderbondStatusBoundTextBox";
			this.CA_UnderbondStatusBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.CA_UnderbondStatusBoundTextBox.TabIndex = 1;
			this.CA_UnderbondStatusBoundTextBox.Text = "";
			this.ContainerControlToolTip.SetToolTip(this.CA_UnderbondStatusBoundTextBox, "Sea Cargo Underbond Status");
			// 
			// SeaCargoHouseUnderbondControl
			// 
			this.Controls.Add(this.ContainerUnderbondMovementGroupBox);
			this.Controls.Add(this.UnderbondHouseBillGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusSCAHouse";
			this.Name = "SeaCargoHouseUnderbondControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 168, true);
			this.ContainerUnderbondMovementGroupBox.ResumeLayout(false);
			this.UnderbondHouseBillGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		protected internal ZGroupBox ContainerUnderbondMovementGroupBox;
		protected internal ZArchitecture.ZLabel zLabel21;
		protected internal ZArchitecture.ZTextBox CN_UnderbondVoyageBoundTextBox;
		protected internal ZCheckBox zCheckBox3;
		protected internal ZCheckBox zCheckBox2;
		protected internal ZArchitecture.ZTextBox CN_UnderbondStatusBoundTextBox;
		protected internal ZAddressControl zAddressControl3;
		protected internal ZAddressControl zAddressControl2;
		protected internal ZArchitecture.ZTextBox CN_MoveUnderbondFromTextBox;
		protected internal ZArchitecture.ZLabel zLabel42;
		protected internal ZArchitecture.ZTextBox CN_MoveUnderbondToTextBox;
		protected internal ZArchitecture.ZLabel zLabel53;
		protected internal ZArchitecture.ZLabel zLabel54;
		protected internal ZGroupBox UnderbondHouseBillGroupBox;
		protected internal ZAddressControl CA_OA_UnderbondFromBoundAddressControl;
		protected internal ZAddressControl CA_OA_UnderbondToBoundAddressControl;
		protected internal ZArchitecture.ZLabel zLabel41;
		protected internal ZCheckBox zCheckBox1;
		protected internal ZArchitecture.ZTextBox CS_MoveUnderbondToTextBox;
		protected internal ZArchitecture.ZTextBox CA_MoveUnderbondFromTextBox;
		protected internal ZArchitecture.ZLabel zLabel49;
		protected internal ZArchitecture.ZLabel zLabel50;
		protected internal ZArchitecture.ZTextBox CA_UnderbondStatusBoundTextBox;
		protected internal ZArchitecture.ZLabel HouseBill;
		protected internal ZArchitecture.ZTextBox zTextBox1;
		protected internal ZArchitecture.ZLabel ContainerNumberLabel;
		protected internal ZArchitecture.ZTextBox CNContainerNumberBoundTextBox;
	}
}
