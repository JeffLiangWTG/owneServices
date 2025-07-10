using System;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUQuarantineEdificeMiscOptionsUserControl : AUEdificeMiscOptionsUserControl
	{
		public AUQuarantineEdificeMiscOptionsUserControl()
		{
			InitializeComponent();
		}

		public override Customs.Business.BaseJobDeclaration JobDeclaration
		{
			set
			{
				var oldDec = JobDeclaration;
				if (oldDec != value)
				{
					if (oldDec != null)
					{
						oldDec.Invoices.CountChanged -= Invoices_CountChanged;
					}
					if (quarantineExDocHeader != null)
					{
						quarantineExDocHeader.QH_ProduceTypeInfo.ValueChanged -= QH_ProduceType_ValueChanged;
						quarantineExDocHeader = null;
					}
					base.JobDeclaration = value;
					var newDec = JobDeclaration;
					if (newDec != null)
					{
						newDec.Invoices.CountChanged -= Invoices_CountChanged;
						newDec.Invoices.CountChanged += Invoices_CountChanged;
						Invoices_CountChanged(null, null);
					}
				}
			}
		}
		QuarantineExDocHeader quarantineExDocHeader;

		void Invoices_CountChanged(object sender, EventArgs e)
		{
			if (JobDeclaration.Invoices.Count == 1)
			{
				if (quarantineExDocHeader == null)
				{
					quarantineExDocHeader = ((JobDeclaration)JobDeclaration).Invoices[0].QuarantineExDocHeader;
					quarantineExDocHeader.QH_ProduceTypeInfo.ValueChanged -= QH_ProduceType_ValueChanged;
					quarantineExDocHeader.QH_ProduceTypeInfo.ValueChanged += QH_ProduceType_ValueChanged;
				}
			}
			else
			{
				if (quarantineExDocHeader != null)
				{
					quarantineExDocHeader.QH_ProduceTypeInfo.ValueChanged -= QH_ProduceType_ValueChanged;
					quarantineExDocHeader = null;
				}
			}
			QH_ProduceType_ValueChanged(null, null);
		}

		void QH_ProduceType_ValueChanged(object sender, EventArgs e)
		{
			var isCertificateRequestCheckBoxVisible = true;
			if (quarantineExDocHeader != null)
			{
				var isNEXDOCSActive = quarantineExDocHeader.IsNEXDOCSActive;
				isCertificateRequestCheckBoxVisible = !isNEXDOCSActive;
			}
			certificateRequestCheckBox.Visible = isCertificateRequestCheckBoxVisible;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			if (JobDeclaration != null)
			{
				JobDeclaration.Invoices.CountChanged -= Invoices_CountChanged;
			}
			if (quarantineExDocHeader != null)
			{
				quarantineExDocHeader.QH_ProduceTypeInfo.ValueChanged -= QH_ProduceType_ValueChanged;
				quarantineExDocHeader = null;
			}
			base.Dispose(disposing);
		}
	}
}
