using System.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Client.JAS.GUI.Cognos
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class CognosAccGLAccountDescriptorForm : AccGLAccountDescriptorForm
	{
		public CognosAccGLAccountDescriptorForm()
		{
			InitializeComponent();
			AllControlsInitialised = true;
		}

		public CognosAccGLAccountDescriptorForm(CognosAccGLAccountDescriptor businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
			AllControlsInitialised = true;

			if (businessEntity != null)
			{
				SetDataBinding(businessEntity, "");
			}
		}

		readonly bool AllControlsInitialised;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (AllControlsInitialised)
			{
				if (DataSource != null)
				{
					DataBindings.RemoveBinding(nameof(IsCognosTabPageVisibleForBinding));
					DataBindings.RemoveBinding(nameof(IsCognosSubClassificationTabPageVisibleForBinding));
					CreditorModuleButtonGrid.DataBindings.RemoveBinding("IsVisibleForBinding");
					DebtorModuleButtonGrid.DataBindings.RemoveBinding("IsVisibleForBinding");
					AccountAgeDropEdit.DataBindings.RemoveBinding("IsVisibleForBinding");
				}
				base.SetDataBinding(dataSource, dataMember);
				if (DataSource != null)
				{
					DataBindings.Add(new KBinding(nameof(IsCognosTabPageVisibleForBinding), BusinessEntity, "IsCognosGLLanguage"));
					DataBindings.Add(new KBinding(nameof(IsCognosSubClassificationTabPageVisibleForBinding), BusinessEntity, "IsCognosSubClassificationAccount"));
					CreditorModuleButtonGrid.DataBindings.Add(new KBinding("IsVisibleForBinding", BusinessEntity, "ExtraInfoForBinding.IsSubClassifiedByCreditor"));
					DebtorModuleButtonGrid.DataBindings.Add(new KBinding("IsVisibleForBinding", BusinessEntity, "ExtraInfoForBinding.IsSubClassifiedByDebtor"));
					AccountAgeDropEdit.DataBindings.Add(new KBinding("IsVisibleForBinding", BusinessEntity, "ExtraInfoForBinding.IsSubClassifiedByAge"));
				}
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBool IsCognosSubClassificationTabPageVisibleForBinding
		{
			get { return (MainTabControl.TabPages.Contains(SubClassificationTabPage)); }
			set
			{
				if (!InCognosSubClassificationTabPageVisibleForBinding)
				{
					InCognosSubClassificationTabPageVisibleForBinding = true;
					if (value)
					{
						if (!MainTabControl.TabPages.Contains(SubClassificationTabPage))
						{
							MainTabControl.TabPages.Insert(SubClassificationTabPage, 2);
						}
					}
					else
					{
						if (MainTabControl.TabPages.Contains(SubClassificationTabPage))
						{
							MainTabControl.TabPages.Remove(SubClassificationTabPage);
						}
					}
					InCognosSubClassificationTabPageVisibleForBinding = false;
				}
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBool IsCognosTabPageVisibleForBinding
		{
			get { return (MainTabControl.TabPages.Contains(CognosTabPage)); }
			set
			{
				if (!InCognosTabPageVisibleForBinding)
				{
					InCognosTabPageVisibleForBinding = true;
					if (value)
					{
						if (!MainTabControl.TabPages.Contains(CognosTabPage))
						{
							MainTabControl.TabPages.Insert(CognosTabPage, 1);
						}
					}
					else
					{
						if (MainTabControl.TabPages.Contains(CognosTabPage))
						{
							MainTabControl.TabPages.Remove(CognosTabPage);
						}
					}
					InCognosTabPageVisibleForBinding = false;
				}
			}
		}

		bool InCognosSubClassificationTabPageVisibleForBinding;
		bool InCognosTabPageVisibleForBinding;

		#region Metadata

		public new static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<CognosAccGLAccountDescriptorForm>()
			.Property("IsCognosTabPageVisibleForBinding", ZBool.True, false)
			.Property("IsCognosSubClassificationTabPageVisibleForBinding", ZBool.True, false)
			.Result;
		}
	#endregion

	internal ZTemplateTabControl InternalMainTabControlTest => MainTabControl;
	internal ZTabPage InternalCognosTabPageTest => CognosTabPage;

	internal ZModuleButtonGrid InternalCreditorModuleButtonGridTest => CreditorModuleButtonGrid;
	internal ZDropEdit InternalAccountAgeDropEditTest => AccountAgeDropEdit;
	internal ZModuleButtonGrid InternalDebtorModuleButtonGridTest => DebtorModuleButtonGrid;
	internal ZTabPage InternalSubClassificationTabPageTest => SubClassificationTabPage;
	}
}
