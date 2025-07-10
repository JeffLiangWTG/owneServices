using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	internal interface IMenuParent
	{
		void BeforeProcessingMenuItem();
	}

	public class ZMenuItem : KMenuItem, IClickableNestedMenuItem, IResCaptionedControl
	{
		public ZMenuItem()
			: base()
		{ }

		public ZMenuItem(string text)
			: base(text)
		{ }

		public ZMenuItem(MultilingualString caption)
		{
			this.Caption = caption;
		}

		public ZMenuItem(string text, EventHandler onClick)
			: base(text, onClick)
		{ }

		public ZMenuItem(MultilingualString caption, EventHandler onClick)
			: this(caption)
		{
			this.Click += onClick;
		}

		public ZMenuItem(ResourceStringData caption)
		{
			this.CaptionResourceString = caption;
		}

		public ZMenuItem(ResourceStringData caption, EventHandler onClick)
			: this(caption)
		{
			this.Click += onClick;
		}

		public ZMenuItem(ResourceStringData caption, IconTypes active, IconTypes rest)
			: this(caption)
		{
			activeIcon = active;
			restIcon = rest;
		}

		public ZMenuItem(ResourceStringData caption, EventHandler onClick, IconTypes active, IconTypes rest)
			: this(caption, onClick)
		{
			activeIcon = active;
			restIcon = rest;
		}

		IconTypes activeIcon;
		public IconTypes ActiveIcon
		{
			get
			{
				return activeIcon;
			}
		}

		IconTypes restIcon;
		public IconTypes RestIcon
		{
			get
			{
				return restIcon;
			}
		}

		public ZMenuItem(string text, EventHandler onClick, Shortcut shortcut)
			: base(text, onClick, shortcut)
		{ }

		public ZMenuItem(MultilingualString caption, EventHandler onClick, Shortcut shortcut)
			: this(caption, onClick)
		{
			this.Shortcut = shortcut;
		}

		public ZMenuItem(string text, MenuItem[] items)
			: base(text, items)
		{ }

		public ZMenuItem(MultilingualString caption, MenuItem[] items)
			: this(caption)
		{
			this.MenuItems.AddRange(items);
		}

		public ResourceStringData CaptionResourceString
		{
			get { return captionResourceString; }
			set
			{
				captionResourceString = value;
				Text = value?.Caption;
				if (value != null && Caption != null && value.Caption != Caption)
				{
					caption = null;
				}
			}
		}
		ResourceStringData captionResourceString;

		public MultilingualString Caption
		{
			get { return caption; }
			set
			{
				caption = value;
				Text = caption;
				if (value != null && CaptionResourceString != null && value != CaptionResourceString.Caption)
				{
					captionResourceString = null;
				}
			}
		}
		MultilingualString caption;

		protected override void OnClick(EventArgs e)
		{
			using (new MenuClickPendingTracker())
			{
				if (TranslationFeedbackManager.InTranslationFeedbackMode())
				{
					OpenFeedbackForm();
				}
				else
				{
					var cmdText = string.Join("/", new MenuTraverser().GetPath(this).Select(x => x.Text));

					(ParentControl as IMenuParent)?.BeforeProcessingMenuItem();

					var form = GetMainMenu()?.GetForm();
					var formName = form?.GetType().FullName;
					var extraKey = formName != null ? ":" + formName : string.Empty;
					using (PerformanceStatisticsCollector.StartMonitoring("MenuClick" + extraKey, cmdText))
					{
						try
						{
							if (ActionSourceCode is { IsEmpty: false } actionSourceCode && form is ZForm { BusinessEntity: { } businessEntity })
							{
								ProcessTemplateValidationManager.Validate(actionSourceCode, businessEntity, () => base.OnClick(e));
							}
							else
							{
								base.OnClick(e);
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException() && form is ZForm zForm)
						{
							zForm.TryHandleSaveException(ex);
						}
					}
				}
			}
		}

		IProcessTemplateValidationManager ProcessTemplateValidationManager => processTemplateValidationManager ??= ObjectFactory.Get<IProcessTemplateValidationManager>();
		IProcessTemplateValidationManager processTemplateValidationManager;

		public ZString ActionSourceCode
		{
			get
			{
				var code = Caption is { } menuItemCaption
					? StripAcceleratorKeys(menuItemCaption.GetUnresolvedString())
					: StripAcceleratorKeys(Text);
				return code;
			}
		}

		protected override void OnInitMenuPopup(EventArgs e)
		{
			if (TranslationFeedbackManager.InTranslationFeedbackMode())
			{
				OpenFeedbackForm();
			}
			else
			{
				base.OnInitMenuPopup(e);
			}
		}

		internal void OpenFeedbackForm()
		{
			OpenFeedbackForm(ParentControl);
		}

		internal void OpenFeedbackForm(Control control)
		{
			if (this.CaptionResourceString != null)
			{
				TranslationFeedbackManager.OpenFeedbackForm(control, this.CaptionResourceString);
			}
			else if (this.Caption != null)
			{
				TranslationFeedbackManager.OpenFeedbackForm(control, this.Caption);
			}
			else
			{
				TranslationFeedbackManager.OpenFeedbackForm(control, this.Text);
			}
		}

		public Control ParentControl
		{
			get
			{
				Control parentControl = null;
				var mainMenu = this.GetMainMenu();
				if (mainMenu != null)
				{
					parentControl = mainMenu.GetForm();
				}
				else
				{
					var contextMenu = this.GetContextMenu();
					if (contextMenu != null)
					{
						parentControl = contextMenu.SourceControl;
					}
				}
				return parentControl;
			}
		}

		public override MenuItem CloneMenu()
		{
			var menuItem = new ZMenuItem();
			menuItem.CloneMenu(this);
			menuItem.Caption = this.Caption;
			menuItem.Text = this.Text;
			if (this.Caption == null && this.CaptionResourceString != null)
			{
				menuItem.CaptionResourceString = this.CaptionResourceString;
			}
			menuItem.activeIcon = this.ActiveIcon;
			menuItem.restIcon = this.RestIcon;
			return menuItem;
		}

		public const string Separator = "-";

		#region IClickableNestedMenuItem Members

		void IClickableNestedMenuItem.AddChildItem(IClickableNestedMenuItem child)
		{
			MenuItems.Add((MenuItem)child);
		}

		Image IClickableNestedMenuItem.Image { get; set; }

		#endregion
	}
}
