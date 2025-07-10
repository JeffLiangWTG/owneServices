using System;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class CHGBaseDeclarationUserControl : ZUserControl
	{
		public CHGBaseDeclarationUserControl()
		{
			InitializeComponent();
		}

		void DecsGrid_AfterBind(object sender, EventArgs e)
		{
			DecsGrid.ListManager.PositionChanged += new EventHandler(DecsGridListManager_PositionChanged);
			DecsGridListManager_PositionChanged(null, null);
		}

		void DecsGridListManager_PositionChanged(object sender, EventArgs e)
		{
			UpdateCurrentDec();
		}

		void UpdateCurrentDec()
		{
			var newDec = (CusTempStorageDec)DecsGrid.ListManager?.GetCurrent();
			if (newDec != null && newDec.IsDeleted)
			{
				newDec = null;
			}

			if (currentDec != newDec)
			{
				UnHookDecEvents();
				currentDec = newDec;
				HookDecEvents();
			}

			void UnHookDecEvents()
			{
				if (currentDec != null)
				{
					currentDec.STH_IdentificationIndicatorInfo.ValueChanged -= STH_IdentificationIndicator_ValueChanged;
				}
			}

			void HookDecEvents()
			{
				if (currentDec != null)
				{
					currentDec.STH_IdentificationIndicatorInfo.ValueChanged += STH_IdentificationIndicator_ValueChanged;
					STH_IdentificationIndicator_ValueChanged(null, null);
				}
			}
		}

		void STH_IdentificationIndicator_ValueChanged(object sender, EventArgs e)
		{
			var isAWBDeclaration = currentDec.IsAWBDeclaration;
			if (isAWBDeclaration)
			{
				currentDec.CusTempStorageLines.MaxCountValidationEnable(1,
					Res.GetString("55506BFD-6A83-4D66-A84E-EDA897F3A407",
					"When Identification Type is '{0}' only a single line is allowed",
					TemporaryStorageIdentificationIndicatorList.Codes.AWB));
			}
			else
			{
				currentDec.CusTempStorageLines.MaxCountValidationDisable();
			}
			SetColumnAvailability(isAWBDeclaration);
			SetControlVisibility(isAWBDeclaration);
		}

		CusTempStorageDec currentDec;

		void AfterBind_LinesGrid(object sender, EventArgs e)
		{
			SetColumnAvailability(currentDec?.IsAWBDeclaration ?? false);
		}

		protected virtual void SetColumnAvailability(bool isAWBDeclaration)
		{
		}

		protected virtual void SetControlVisibility(bool isAWBDeclaration)
		{
		}
	}
}
