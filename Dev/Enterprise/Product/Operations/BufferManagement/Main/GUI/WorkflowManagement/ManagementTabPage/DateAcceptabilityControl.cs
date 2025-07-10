using System;
using System.Drawing;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class DateAcceptabilityControl : ZUserControl
	{
		public DateAcceptabilityControl()
		{
			InitializeComponent();
		}

		public new IDateAcceptability DataSource
		{
			get { return BindingSource.Current as IDateAcceptability; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource == null)
			{
				UnsubscribeValueChanged();
			}
			else
			{
				var bindingManager = BindingContext[dataSource, dataMember];
				if (bindingManager != null)
				{
					bindingManager.CurrentChanged += BindingManager_CurrentChanged;
					BindingManager_CurrentChanged(this, EventArgs.Empty);
				}
			}
		}

		void BindingManager_CurrentChanged(object sender, EventArgs e)
		{
			UnsubscribeValueChanged();
			SubscribeValueChanged();
			SetDateAcceptabilityGraphic();
		}

		void SubscribeValueChanged()
		{
			if (DataSource != null)
			{
				DataSource.FH_DateAcceptabilityInfo.ValueChanged += ProcessHeader_DateAcceptabilityChanged;
			}
		}

		void UnsubscribeValueChanged()
		{
			if (DataSource != null)
			{
				DataSource.FH_DateAcceptabilityInfo.ValueChanged -= ProcessHeader_DateAcceptabilityChanged;
			}
		}

		void ProcessHeader_DateAcceptabilityChanged(object sender, EventArgs e)
		{
			SetDateAcceptabilityGraphic();
		}

		void SetDateAcceptabilityGraphic()
		{
			if (!IsDisposed)
			{
				if (DataSource != null && !DataSource.IsDeleted)
				{
					var image = DateAcceptabilityImages.Get(DataSource.FH_DateAcceptability);
					image = image != null ? new Bitmap(image, DateAcceptabilityPictureBox.Size) : null;
					DateAcceptabilityPictureBox.Image = image;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (DataSource != null)
					{
						DataSource.FH_DateAcceptabilityInfo.ValueChanged -= ProcessHeader_DateAcceptabilityChanged;
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		void LegendButton_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new DateAcceptabilityLegendForm());
		}
	}
}
