using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.Declaration.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class BaseAddInfoControl /*this is a control that could be bound to any business object*/ : ZUserControl, IExtendedControl, IBindTo
	{
		#region Custom Adornment Layout

		class BaseAddInfoControlAdornmentLayout : AdornmentLayout<BaseAddInfoControl>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(BaseAddInfoControl source)
			{
				yield return source.AddInfoTextBox;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(BaseAddInfoControl source)
			{
				yield return new IconLayout(source.AddInfoButton, IconAlignment.Center);
			}
		}

		#endregion

		#region Constructors

		static BaseAddInfoControl()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new BaseAddInfoControlAdornmentLayout());
		}

		public BaseAddInfoControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				AddInfoButton.Font = OFont.GetFontBold();
				AddInfoButton.Click += AddInfoButton_Click;
				AddInfoButton.TabStop = false;
			}
			Extensions = new DefaultControlExtensionCollection(this);
			AddInfoTextBox.ReadOnlyChanged += new EventHandler(AddInfoTextBox_ReadOnlyChanged);
		}

		#endregion

		#region Virtual

		protected virtual void AddInfoButton_Click(object sender, EventArgs e)
		{
		}

		#endregion

		#region ReadOnly

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnly
		{
			get { return AddInfoTextBox.ReadOnly; }
		}

		#endregion

		#region IDataBoundControl Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindTo
		{
			get { return this.GetBindingMember(); }
			set { this.SetBindingMember(value); }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			AddInfoTextBox.SetDataBinding(dataSource, dataMember);
			DataBoundControl.SetDataBindingForMetadataProperties(this, dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		void AddInfoTextBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			AddInfoButton.ReadOnly = AddInfoTextBox.ReadOnly;
		}

		public override Type DataSourceType
		{
			get { return typeof(string); }
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region GetPropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<BaseAddInfoControl>()
			.Property("ReadOnly", false, false)
			.Result;
		}

		#endregion

		#region Implementation

		protected internal AUAddInfo CurrentAddInfo
		{
			get
			{
				BindingManagerBase bm = AddInfoTextBox.DataBindings["Text"].BindingManagerBase;
				object currentObject = bm.Position == -1 ? null : bm.GetCurrent();
				var result = currentObject as AUAddInfo;
				if (result == null && currentObject is IAddInfo iAddInfo)
				{
					result = iAddInfo.AddInfo;
				}
				return result;
			}
		}

		#endregion
	}
}
