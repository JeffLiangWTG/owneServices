using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[CodeProperty(Schema.FM_Name), DescriptionProperty(Schema.FM_Name)]
	public class BMControlCustomisation : AutoBMControlCustomisation,
		IDocManagerSupport,
		IPreviewNotifier,
		ITemplateCopyable,
		IBMControlCustomisation,
		IAuditParent
	{
		public BMControlCustomisation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region FM_ControlType

		[List("Lookups.ControlTypes")]
		public override ZString FM_ControlType
		{
			get { return base.FM_ControlType; }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(FM_ControlType, value))
				{
					base.FM_ControlType = value;
				}
			}
		}

		#endregion

		#region FM_JobType

		[List("Lookups.JobTypes")]
		public override ZString FM_JobType
		{
			get { return base.FM_JobType; }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(FM_JobType, value))
				{
					base.FM_JobType = value;
				}
			}
		}

		#endregion

		#endregion

		#region Xml Properties

		#region Width

		[XmlColumnProperty]
		[ResourceStringData("BMControlCustomisation.Width", Caption = "Width")]
		public ZInt Width
		{
			get { return GetXmlColumnPropertyValue<ZInt>(WidthInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(Width, value))
				{
					SetXmlColumnPropertyValue(WidthInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateWidth();
					}
				}
			}
		}

		public ZPropertyInfo WidthInfo
		{
			get { return GetZPropertyInfo(nameof(Width)); }
		}

		#endregion

		#region Height

		[XmlColumnProperty]
		[ResourceStringData("BMControlCustomisation.Height", Caption = "Height")]
		public ZInt Height
		{
			get { return GetXmlColumnPropertyValue<ZInt>(HeightInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(Height, value))
				{
					SetXmlColumnPropertyValue(HeightInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateHeight();
					}
				}
			}
		}

		public ZPropertyInfo HeightInfo
		{
			get { return GetZPropertyInfo(nameof(Height)); }
		}

		#endregion

		#region BackgroundColor

		[XmlColumnProperty]
		[List("Lookups.ColorList")]
		[ResourceStringData("BMControlCustomisationLine.BackgroundColor", Caption = "Background Color", ShortCaption = "Background")]
		[MaxLength(50)]
		public ZString BackgroundColor
		{
			get { return GetXmlColumnPropertyValue<ZString>(BackgroundColorInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(BackgroundColor, value))
				{
					SetXmlColumnPropertyValue(BackgroundColorInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateBackgroundColor();
					}
				}
			}
		}

		public ZPropertyInfo BackgroundColorInfo
		{
			get { return GetZPropertyInfo(nameof(BackgroundColor)); }
		}

		public Color BackgroundColorValue
		{
			get { return ColorList.ColorFromName(BackgroundColor); }
		}

		#endregion

		#region BackgroundImage

		[XmlColumnProperty]
		public ZBlob BackgroundImage
		{
			get { return GetXmlColumnPropertyValue<ZBlob>(BackgroundImageInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(BackgroundImage, value))
				{
					SetXmlColumnPropertyValue(BackgroundImageInfo, value);
				}
			}
		}

		public ZPropertyInfo BackgroundImageInfo
		{
			get { return GetZPropertyInfo(nameof(BackgroundImage)); }
		}

		public Image BackgroundImageValue
		{
			get { return GetImage(BackgroundImage); }
			set
			{
				if (value != null)
				{
					using (var stream = new MemoryStream())
					{
						value.Save(stream, ImageFormat.Bmp);
						BackgroundImage = (ZBlob)stream.ToArray();
					}
				}
				else
				{
					BackgroundImage = ZBlob.Empty;
				}
			}
		}

		static Image GetImage(ZBlob blob)
		{
			return !blob.IsEmpty ? Image.FromStream(new MemoryStream(blob)) : null;
		}

		#endregion

		#endregion

		#region New Properties

		[ResourceStringData("BMControlCustomisation.TypeDescription", Caption = "Control Type", ShortCaption = "Type", FullDescription = "The type of graphical element to override on a Visual Board.")]
		public ZString TypeDescription
		{
			get { return Lookups.ControlTypes.GetDescriptionFromCode(FM_ControlType); }
		}

		[ResourceStringData("BMControlCustomisation.JobTypeDescription", Caption = "Job Type", FullDescription = "The type of job this layout will be used for on a Visual Board.")]
		public ZString JobTypeDescription
		{
			get { return Lookups.JobTypes.GetDescriptionFromCode(FM_JobType); }
		}

		[XmlColumnProperty]
		[ResourceStringData("BMControlCustomisationLine.RenderOnTheWeb",
			Caption = "Attempts to render this layout on the web when using a Web PAVE Board",
			ShortCaption = "Render this layout for Web PAVE")]
		public ZBool RenderOnTheWeb
		{
			get { return GetXmlColumnPropertyValue<ZBool>(RenderOnTheWebInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(RenderOnTheWeb, value))
				{
					SetXmlColumnPropertyValue(RenderOnTheWebInfo, value);
				}
			}
		}

		public ZPropertyInfo RenderOnTheWebInfo
		{
			get { return GetZPropertyInfo(nameof(RenderOnTheWeb)); }
		}

		#endregion

		#region Related business objects

		[ChildEditable]
		[XmlColumnProperty]
		public BMControlCustomisationLineCollection CustomisationLines
		{
			get
			{
				if (customisationLines == null)
				{
					customisationLines = new BMControlCustomisationLineCollection(this);
					RegisterEditableChildObject(customisationLines);
				}

				return customisationLines;
			}
		}

		BMControlCustomisationLineCollection customisationLines;

		[ChildEditable]
		[XmlColumnProperty]
		public StaticControlCustomisationCollection CustomisedControls
		{
			get
			{
				if (customisedControls == null)
				{
					customisedControls = new StaticControlCustomisationCollection(this);
					RegisterEditableChildObject(customisedControls);
				}

				return customisedControls;
			}
		}

		StaticControlCustomisationCollection customisedControls;

		[ChildEditable]
		public BMControlCustomisationUsagesCollection ControlUsages
		{
			get
			{
				if (controlUsages == null)
				{
					controlUsages = new BMControlCustomisationUsagesCollection(this);
					RegisterEditableChildObject(controlUsages);
				}

				return controlUsages;
			}
		}

		BMControlCustomisationUsagesCollection controlUsages;

		#endregion

		#region IDocManagerSupport

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.BMControlCustomisation)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IPreviewNotifier

		public IPreviewReceiver PreviewReceiver { get; set; }

		#endregion

		#region ITemplateCopyable

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			return Clone();
		}

		#endregion

		#region BusinessObject overrides

		public override string ToString() => string.Format(CultureInfo.InvariantCulture, (NoResString)"Name: {0}, Job Type: {1}, Control Type: {2}", FM_Name, FM_JobType, FM_ControlType); // String representation on an object

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			Width = 300;
			Height = 200;
		}

		protected override IEnumerable<XmlColumnSpecification> XmlSerialisedColumns
		{
			get { yield return new XmlColumnSpecification(BMControlCustomisationSchema.FM_LayoutData); }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("5397fccf-7b80-41f4-9b61-22f3ed1f7886", "Visual Layout") + (FM_Name.IsEmpty ? string.Empty : " - " + FM_Name); }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (BMControlCustomisation)base.CloneInternal(args);

			clone.AppendNameForClone();

			return clone;
		}

		public void AppendNameForClone()
		{
			var copyText = " - " + Res.GetString("68f6e940-82db-4380-8b7b-8b303b3636a0", "Copy");

			if (FM_Name.Length + copyText.Length <= BMControlCustomisationSchema.FM_Name.MaxLength)
			{
				FM_Name += copyText;
			}

			if (FM_NameInfo.HasErrors())
			{
				var startingName = FM_Name;
				for (int i = 1; FM_NameInfo.HasErrors(); i++)
				{
					FM_Name = startingName + " " + i;
				}
			}
		}

		public override bool IsSavedByFactory => base.IsSavedByFactory && AllowSavingByFactory;

		public bool AllowSavingByFactory { get; set; } = true;

		protected override bool ShouldUpdateNaturalKeyCacheWhenFM_ControlTypeChanges(ZString newValue)
		{
			return FM_IsSystemWide;
		}

		#endregion

		#region System Defaults

		public static BMControlCustomisation GetNewDefaultCardLayout(BusinessObjectFactory factory, string cardType, bool allowSavingByFactory = true)
		{
			BMControlCustomisation result;
			using (factory.TemporarilyDisableValidation())
			{
				switch (cardType)
				{
					case CustomisedControlTypeList.Codes.DetailedCard:
						result = GetNewDefaultDetailedTaskCard(factory);
						break;

					case CustomisedControlTypeList.Codes.TaskCard:
						result = GetNewDefaultSummaryTaskCard(factory);
						break;

					case CustomisedControlTypeList.Codes.WorkflowDetailedCard:
						result = GetNewDefaultDetailedWorkflowCard(factory);
						break;

					case CustomisedControlTypeList.Codes.WorkflowSummaryCard:
						result = GetNewDefaultSummaryWorkflowCard(factory);
						break;

					default:
						throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Card type [{0}] is not supported", cardType));
				}
			}

			result.AllowSavingByFactory = allowSavingByFactory;
			return result;
		}

		#region SummaryTaskCard

		static BMControlCustomisation GetNewDefaultSummaryTaskCard(BusinessObjectFactory factory)
		{
			var customisation = factory.New<BMControlCustomisation>();

			customisation.FM_Name = Res.GetString("7894e541-6fb7-4bec-adbd-ad5afc6e0ce9", "System Default Summary Task Card");
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
			customisation.Width = 122;
			customisation.Height = 57;
			customisation.BackgroundColor = ColorList.NameFromColor(Color.White);

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "ProviderJobNumber",
				ControlType = PropertyTypeList.Codes.Text,
				IsBold = true,
				IsReadOnly = true,
				BringToFront = true,
				AutoSize = true,
				Left = 2,
				Top = 2,
				Width = 0,
				Height = 16,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.ProcessTask,
				PropertyName = "RelevantEstimateHoursLabel",
				ControlType = PropertyTypeList.Codes.Text,
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = true,
				Alignment = ControlAlignmentList.Codes.Right,
				Left = 82,
				Top = 2,
				Width = 0,
				Height = 16,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "ProviderJobDescription",
				ControlType = PropertyTypeList.Codes.Text,
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = true,
				Left = 2,
				Top = 18,
				Width = 0,
				Height = 16,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.ProcessTask,
				PropertyName = ProcessTasksSchema.P9_Description.Name,
				ControlType = PropertyTypeList.Codes.Text,
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = true,
				Left = 2,
				Top = 34,
				Width = 0,
				Height = 16,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.TaskStatusIndicator,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = true,
				Alignment = ControlAlignmentList.Codes.Right,
				Orientation = nameof(BMBoardSectionOrientation.Vertical),
				Left = 110,
				Top = 2,
				Width = 12,
				Height = 12,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.AttachedTagsIndicator,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = true,
				Left = 1,
				Top = 48,
				Width = 120,
				Height = 11,
			});

			return customisation;
		}

		#endregion

		#region SummaryWorkflowCard

		static BMControlCustomisation GetNewDefaultSummaryWorkflowCard(BusinessObjectFactory factory)
		{
			var customisation = factory.New<BMControlCustomisation>();

			customisation.FM_Name = Res.GetString("61f180f4-9121-41cd-8d97-fd2aa55dcacc", "System Default Summary Workflow Card");
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowSummaryCard;
			customisation.Width = 122;
			customisation.Height = 57;
			customisation.BackgroundColor = ColorList.NameFromColor(Color.White);

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "ProviderJobNumber",
				ControlType = PropertyTypeList.Codes.Text,
				IsBold = true,
				IsReadOnly = true,
				BringToFront = true,
				AutoSize = true,
				Left = 2,
				Top = 2,
				Width = 0,
				Height = 16,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "TotalRelevantEstimatedHoursSummary",
				ControlType = PropertyTypeList.Codes.Text,
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = true,
				Alignment = ControlAlignmentList.Codes.Right,
				Left = 82,
				Top = 2,
				Width = 0,
				Height = 16,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "ProviderJobDescription",
				ControlType = PropertyTypeList.Codes.Text,
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = true,
				Left = 2,
				Top = 18,
				Width = 0,
				Height = 16,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = ProcessHeaderSchema.FH_CompletionStatement.Name,
				ControlType = PropertyTypeList.Codes.Text,
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = true,
				Left = 2,
				Top = 34,
				Width = 0,
				Height = 16,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.TaskStatusIndicator,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = true,
				Alignment = ControlAlignmentList.Codes.Right,
				Orientation = nameof(BMBoardSectionOrientation.Vertical),
				Left = 110,
				Top = 2,
				Width = 12,
				Height = 12,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.AttachedTagsIndicator,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = true,
				Left = 1,
				Top = 48,
				Width = 120,
				Height = 11,
			});

			return customisation;
		}

		#endregion

		#region DetailedTaskCard

		static BMControlCustomisation GetNewDefaultDetailedTaskCard(BusinessObjectFactory factory)
		{
			var customisation = factory.New<BMControlCustomisation>();

			customisation.FM_Name = Res.GetString("abdd1481-c84f-4669-9c90-8cda7b0f8625", "System Default Detailed Task Card");
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;
			customisation.Width = 350;
			customisation.Height = 238;
			customisation.BackgroundColor = ColorList.NameFromColor(Color.White);

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "ProviderJobDescription",
				ControlType = PropertyTypeList.Codes.Text,
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = false,
				Left = 110,
				Top = 1,
				Width = 235,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "ProviderJobNumber",
				ControlType = PropertyTypeList.Codes.Text,
				IsBold = true,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = true,
				Left = 30,
				Top = 3,
				Width = 80,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.ProcessTask,
				PropertyName = "StatusDescription",
				ControlType = PropertyTypeList.Codes.Text,
				Label = Res.GetString("e9f13f5a-3036-4664-81e0-ba8293624191", "Status"),
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = false,
				Left = 65,
				Top = 25,
				Width = 250,
				Height = 25,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = ProcessHeaderSchema.Constants.FH_CompletionStatement,
				ControlType = PropertyTypeList.Codes.Text,
				Label = Res.GetString("d8e081ad-51da-4ae8-bd3f-3f4b091f5226", "Workflow"),
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = false,
				Left = 65,
				Top = 50,
				Width = 235,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.ProcessTask,
				PropertyName = ProcessTasksSchema.Constants.P9_Description,
				ControlType = PropertyTypeList.Codes.Text,
				Label = Res.GetString("7f75d37b-9dce-481f-b13d-62088ec42efa", "Task:"),
				BackgroundColor = Color.LightYellow,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				AutoSize = false,
				Left = 65,
				Top = 75,
				Width = 275,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.ProcessTask,
				PropertyName = "EstimateDescription",
				ControlType = PropertyTypeList.Codes.Text,
				Label = Res.GetString("fa53ac26-026d-4d17-9128-72df192a4e06", "Estimate"),
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = false,
				Left = 65,
				Top = 100,
				Width = 350,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.ProcessTask,
				PropertyName = "EstimatedHandoverTimeLocal",
				ControlType = PropertyTypeList.Codes.DateTime,
				Label = Res.GetString("9ac484fd-acf8-4ef3-bc6d-a582187a8286", "Handover:"),
				IsBold = false,
				IsReadOnly = false,
				BringToFront = true,
				AutoSize = false,
				Left = 65,
				Top = 125,
				Width = 120,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.ProcessTask,
				PropertyName = ProcessTasksSchema.Constants.P9_ScheduledDate,
				ControlType = PropertyTypeList.Codes.DateTime,
				Label = Res.GetString("5a83118a-ceba-4006-9fef-754b35454334", "Reminder:"),
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				AutoSize = false,
				Left = 65,
				Top = 150,
				Width = 120,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.ProcessTask,
				PropertyName = ProcessTasksSchema.Constants.P9_IsCalendarItem,
				ControlType = PropertyTypeList.Codes.Boolean,
				Label = Res.GetString("c5cc4ba7-459b-492c-ae71-58362d516213", "Add to Calendar"),
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				AutoSize = false,
				Left = 200,
				Top = 150,
				Width = 130,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.ProcessTask,
				PropertyName = ProcessTasksSchema.Constants.P9_CardNote,
				ControlType = PropertyTypeList.Codes.Text,
				Label = Res.GetString("44b3601c-2f67-4278-989a-6b855792736d", "Card Note:"),
				BackgroundColor = Color.LightYellow,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				AutoSize = false,
				Left = 65,
				Top = 175,
				Width = 275,
				Height = 20,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.CloseButton,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = true,
				Left = 323,
				Top = 2,
				Width = 25,
				Height = 25,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.TaskStatusIndicator,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				Alignment = ControlAlignmentList.Codes.Right,
				Left = 0,
				Top = 3,
				Width = 12,
				Height = 12,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.Label,
				Label = Res.GetString("c31d9999-429a-44f7-ba1d-fdaa218c5f15", "Tag:"),
				IsBold = false,
				IsReadOnly = false,
				BringToFront = true,
				Alignment = ControlAlignmentList.Codes.Right,
				Left = 2,
				Top = 196,
				Width = 60,
				Height = 18,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.AttachedTagsIndicator,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = true,
				Left = 65,
				Top = 200,
				Width = 270,
				Height = 10,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.StatusButtons,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				Left = 0,
				Top = 214,
				Width = 211,
				Height = 26,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.CapabilityAssignmentButton,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = true,
				Left = 4,
				Top = 214,
				Width = 60,
				Height = 22,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.OpenJobButton,
				Label = Res.GetString("cba31970-b9ed-4aae-a725-621aaa63dd31", "Open"),
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				Left = 215,
				Top = 214,
				Width = 60,
				Height = 22,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.SaveButton,
				Label = Res.GetString("32df1642-6c1b-4cfe-976a-72abca5d9b73", "Save"),
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				Left = 280,
				Top = 214,
				Width = 60,
				Height = 22,
			});

			return customisation;
		}

		#endregion

		#region DetailedWorkflowCard

		static BMControlCustomisation GetNewDefaultDetailedWorkflowCard(BusinessObjectFactory factory)
		{
			var customisation = factory.New<BMControlCustomisation>();

			customisation.FM_Name = Res.GetString("1a331de5-76ed-4d8c-a8e6-7dfaa5720c4e", "System Default Detailed Workflow Card");
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowDetailedCard;
			customisation.Width = 350;
			customisation.Height = 238;
			customisation.BackgroundImage = (byte[])new ImageConverter().ConvertTo(Properties.Resources.TaskCard, typeof(byte[]));
			customisation.BackgroundColor = ColorList.NameFromColor(Color.LightYellow);

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "ProviderJobNumber",
				ControlType = PropertyTypeList.Codes.Text,
				IsBold = true,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = false,
				Left = 0,
				Top = 0,
				Width = 80,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "ProviderJobDescription",
				ControlType = PropertyTypeList.Codes.Text,
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = false,
				Left = 85,
				Top = 0,
				Width = 235,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = ProcessHeaderSchema.Constants.FH_CompletionStatement,
				ControlType = PropertyTypeList.Codes.Text,
				Label = Res.GetString("1d33e642-5520-4612-a6a8-7ee50354137c", "Description"),
				BackgroundColor = Color.LightYellow,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = true,
				AutoSize = false,
				Left = 85,
				Top = 20,
				Width = 235,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "CurrentTasksStatus",
				ControlType = PropertyTypeList.Codes.Text,
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = true,
				Left = 85,
				Top = 45,
				Width = 0,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "TotalEstimatedHoursSummary",
				ControlType = PropertyTypeList.Codes.Text,
				Label = Res.GetString("fa53ac26-026d-4d17-9128-72df192a4e06", "Estimate"),
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = true,
				Left = 85,
				Top = 62,
				Width = 0,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "TotalActualHoursSummary",
				ControlType = PropertyTypeList.Codes.Text,
				Label = Res.GetString("b19350f4-d81a-4ba9-a829-1eb30a1f9293", "Actual Hours"),
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = true,
				Left = 85,
				Top = 79,
				Width = 0,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "PrerequisiteStatusShortDescription",
				ControlType = PropertyTypeList.Codes.Text,
				Label = Res.GetString("715be8d7-dd41-4b38-aec5-e34d374d7c77", "Prerequisite Status"),
				IsBold = false,
				IsReadOnly = true,
				BringToFront = false,
				AutoSize = true,
				Left = 110,
				Top = 97,
				Width = 0,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "DoNotStartBeforeDateLocal",
				ControlType = PropertyTypeList.Codes.DateTime,
				Label = Res.GetString("8d4ff3c5-8e48-4510-81b9-431eeca63b62", "Earliest Start"),
				BackgroundColor = Color.LightYellow,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = true,
				AutoSize = false,
				Left = 85,
				Top = 111,
				Width = 115,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = "AgreedDeliveryDateLocal",
				ControlType = PropertyTypeList.Codes.DateTime,
				Label = Res.GetString("68612213-2623-4b98-a187-3f7a233dc1eb", "Delivery Date"),
				BackgroundColor = Color.LightYellow,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = true,
				AutoSize = false,
				Left = 85,
				Top = 132,
				Width = 115,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = ProcessHeaderSchema.Constants.FH_IsActive,
				ControlType = PropertyTypeList.Codes.Boolean,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				AutoSize = false,
				Left = 85,
				Top = 155,
				Width = 60,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = ProcessHeaderSchema.Constants.FH_IsStandby,
				ControlType = PropertyTypeList.Codes.Boolean,
				Label = Res.GetString("54ac36fb-7040-4505-b7d8-c4a9bbd7def0", "Standby Task"),
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				AutoSize = false,
				Left = 210,
				Top = 155,
				Width = 140,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = ProcessHeaderSchema.Constants.FH_IsCriticalHandover,
				ControlType = PropertyTypeList.Codes.Boolean,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				AutoSize = false,
				Left = 85,
				Top = 174,
				Width = 110,
				Height = 20,
			});

			CreateCustomisationLine(customisation, new ControlCustomisationDTO
			{
				PropertySource = PropertySourceList.Codes.Workflow,
				PropertyName = ProcessHeaderSchema.Constants.FH_AllowTaskAutoAssignment,
				ControlType = PropertyTypeList.Codes.Boolean,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				AutoSize = false,
				Left = 210,
				Top = 174,
				Width = 120,
				Height = 20,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.CloseButton,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				Left = 323,
				Top = 2,
				Width = 25,
				Height = 25,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.TaskStatusIndicator,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				Alignment = ControlAlignmentList.Codes.Right,
				Left = 70,
				Top = 46,
				Width = 12,
				Height = 12,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.NudgeControls,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				Left = 210,
				Top = 93,
				Width = 84,
				Height = 26,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.DateAcceptabilityPicture,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = true,
				Left = 220,
				Top = 118,
				Width = 58,
				Height = 20,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.AttachedTagsIndicator,
				IsBold = false,
				IsReadOnly = false,
				BringToFront = true,
				Left = 0,
				Top = 196,
				Width = 352,
				Height = 15,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.OpenJobButton,
				Label = Res.GetString("ad3a5f62-5f57-4c99-ac1b-c12c44bd2f7e", "Open Job"),
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				Left = 240,
				Top = 214,
				Width = 61,
				Height = 22,
			});

			CreateCustomisedControl(customisation, new ControlCustomisationDTO
			{
				ControlType = StaticControlTypeList.Codes.SaveButton,
				Label = Res.GetString("32df1642-6c1b-4cfe-976a-72abca5d9b73", "Save"),
				IsBold = false,
				IsReadOnly = false,
				BringToFront = false,
				Left = 305,
				Top = 214,
				Width = 40,
				Height = 22,
			});

			return customisation;
		}

		#endregion

		#region Implementation

		static void CreateCustomisationLine(BMControlCustomisation customisation, ControlCustomisationDTO spec)
		{
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = spec.PropertySource;
			line.PropertyName = spec.PropertyName;
			line.AutoSize = spec.AutoSize;

			SetCustomisationProperties(line, spec);
		}

		static void CreateCustomisedControl(BMControlCustomisation customisation, ControlCustomisationDTO spec)
		{
			var control = customisation.CustomisedControls.AddNew();
			SetCustomisationProperties(control, spec);
		}

		static void SetCustomisationProperties(ControlCustomisationBase customisationLine, ControlCustomisationDTO spec)
		{
			customisationLine.Alignment = spec.Alignment;
			customisationLine.ControlType = spec.ControlType;
			customisationLine.Label = spec.Label;
			customisationLine.Left = spec.Left;
			customisationLine.Top = spec.Top;
			customisationLine.Width = spec.Width;
			customisationLine.Height = spec.Height;

			if (spec.BackgroundColor != null)
			{
				customisationLine.BackgroundColor = ColorList.NameFromColor(spec.BackgroundColor.Value);
			}

			if (spec.ForegroundColor != null)
			{
				customisationLine.ForegroundColor = ColorList.NameFromColor(spec.ForegroundColor.Value);
			}

			customisationLine.Font = spec.Font;
			customisationLine.FontSize = spec.FontSize;
			customisationLine.IsBold = spec.IsBold;
			customisationLine.IsReadOnly = spec.IsReadOnly;
			customisationLine.BringToFront = spec.BringToFront;
			customisationLine.Orientation = spec.Orientation;
		}

		class ControlCustomisationDTO
		{
			internal ControlCustomisationDTO()
			{
				Font = Label = Orientation = string.Empty;
				Alignment = ControlAlignmentList.Codes.Left;
				FontSize = 8;
				BackgroundColor = Color.Transparent;
				ForegroundColor = Color.Black;
			}

			internal string Alignment { get; set; }
			internal string ControlType { get; set; }
			internal string Label { get; set; }
			[SuppressMessage("Microsoft.Performance", "CA1811", Justification = "Property accessed through reflection")]
			internal int Left { get; set; }
			[SuppressMessage("Microsoft.Performance", "CA1811", Justification = "Property accessed through reflection")]
			internal int Top { get; set; }
			[SuppressMessage("Microsoft.Performance", "CA1811", Justification = "Property accessed through reflection")]
			internal int Width { get; set; }
			[SuppressMessage("Microsoft.Performance", "CA1811", Justification = "Property accessed through reflection")]
			internal int Height { get; set; }
			internal Color? BackgroundColor { get; set; }
			internal Color? ForegroundColor { get; set; }
			internal string Font { get; set; }
			internal int FontSize { get; set; }
			internal bool IsBold { get; set; }
			internal bool IsReadOnly { get; set; }
			internal bool BringToFront { get; set; }
			internal string Orientation { get; set; }

			internal string PropertySource { get; set; }
			internal string PropertyName { get; set; }
			internal bool AutoSize { get; set; }
		}

		#endregion

		#endregion

		#region ApplicableCustomisation

		public static BMControlCustomisation GetCustomisedDetailedCard(BMComponentSectionConfiguration boardSectionConfiguration, string jobType)
		{
			return GetCustomisedLayout(GetAllPossibleCustomisedDetailedCardLinks(boardSectionConfiguration), jobType);
		}

		public static BMControlCustomisation GetCustomisedSummaryCard(BMComponentSectionConfiguration boardSectionConfiguration, string jobType)
		{
			return GetCustomisedLayout(GetAllPossibleCustomisedSummaryCardLinks(boardSectionConfiguration), jobType);
		}

		static BMControlCustomisation GetCustomisedLayout(IEnumerable<BMControlCustomisationLink> links, string jobType)
		{
			var link = links.SingleOrDefault(c => c.FML_JobType == jobType || c.FML_JobType.IsEmpty);

			return link != null ? link.CustomisedLayout : null;
		}

		internal static IEnumerable<BMControlCustomisationLink> GetAllPossibleCustomisedDetailedCardLinks(BMComponentSectionConfiguration boardSectionConfiguration)
		{
			var controlType = boardSectionConfiguration.ShowWorkflowOrJobWorkflowCards ? CustomisedControlTypeList.Codes.WorkflowDetailedCard : CustomisedControlTypeList.Codes.DetailedCard;
			return GetControlCustomisationLinks(boardSectionConfiguration, controlType);
		}

		internal static IEnumerable<BMControlCustomisationLink> GetAllPossibleCustomisedSummaryCardLinks(BMComponentSectionConfiguration boardSectionConfiguration)
		{
			var controlType = boardSectionConfiguration.ShowWorkflowOrJobWorkflowCards ? CustomisedControlTypeList.Codes.WorkflowSummaryCard : CustomisedControlTypeList.Codes.TaskCard;
			return GetControlCustomisationLinks(boardSectionConfiguration, controlType);
		}

		static IEnumerable<BMControlCustomisationLink> GetControlCustomisationLinks(BMComponentSectionConfiguration boardSectionConfiguration, string controlType)
		{
			var results = new Dictionary<ZString, BMControlCustomisationLink>();

			foreach (var link in GetPotentialControlCustomisationLinks(boardSectionConfiguration, controlType))
			{
				if (!results.ContainsKey(link.FML_JobType))
				{
					results.Add(link.FML_JobType, link);
				}

				if (link.FML_ControlType.IsEmpty)
				{
					break;
				}
			}

			return results.Values;
		}

#if DEBUG
		public
#endif
			static IEnumerable<BMControlCustomisationLink> GetPotentialControlCustomisationLinks(BMComponentSectionConfiguration boardSectionConfiguration, string controlType)
		{
			var fetchHintList = new List<BMControlCustomisationLink>();

			AddControlCustomisationLinkFetchHints(boardSectionConfiguration.Factory, boardSectionConfiguration.Section.PK);

			var board = boardSectionConfiguration.Section.Board;
			if (board != null)
			{
				AddControlCustomisationLinkFetchHints(boardSectionConfiguration.Factory, board.PK);
			}

			var releaseGroupPK = boardSectionConfiguration.ApplicableReleaseGroupPK;
			if (releaseGroupPK.IsValid)
			{
				var group = boardSectionConfiguration.Factory.Load<GlbGroup>(releaseGroupPK);
				if (group != null)
				{
					var customisedLayoutLinks = new BMControlCustomisationLinkCollection(group);
					AddControlCustomisationLinkFetchHints(boardSectionConfiguration.Factory, group.PK);
				}
			}

			var systemPk = board != null ? board.MB_FS_System : ZGuid.Empty;

			if (systemPk.IsValid)
			{
				AddControlCustomisationLinkFetchHints(boardSectionConfiguration.Factory, systemPk);
			}

			var sectionList = ((BMControlCustomisationLinkCollection)((ICustomisedLayoutSupportable)boardSectionConfiguration.Section).CustomisedLayoutLinks).ToList();
			fetchHintList.AddRange(sectionList);

			var boardList = new List<BMControlCustomisationLink>();
			if (board != null)
			{
				boardList = board.CustomisedLayoutLinks.ToList();
				fetchHintList.AddRange(boardList);
			}

			var groupList = new List<BMControlCustomisationLink>();
			if (releaseGroupPK.IsValid)
			{
				var group = boardSectionConfiguration.Factory.Load<GlbGroup>(releaseGroupPK);
				if (group != null)
				{
					var customisedLayoutLinks = new BMControlCustomisationLinkCollection(group);
					groupList = customisedLayoutLinks.ToList();
					fetchHintList.AddRange(groupList);
				}
			}

			var systemList = new List<BMControlCustomisationLink>();

			if (systemPk.IsValid)
			{
				systemList = board.Factory.Load<BMControlCustomisationLink>(new ZQuery(BMControlCustomisationLinkSchema.FML_ParentId, systemPk)).ToList();
				fetchHintList.AddRange(systemList);
			}

			BMControlCustomisationLink.AddBMControlCustomisationFetchHints(fetchHintList, boardSectionConfiguration.Factory);

			foreach (BMControlCustomisationLink link in GetPotentialLinksList(sectionList, controlType))
			{
				yield return link;
			}

			foreach (BMControlCustomisationLink link in GetPotentialLinksList(boardList, controlType))
			{
				yield return link;
			}

			foreach (BMControlCustomisationLink link in GetPotentialLinksList(groupList, controlType))
			{
				yield return link;
			}

			foreach (BMControlCustomisationLink link in GetPotentialLinksList(systemList, controlType))
			{
				yield return link;
			}
		}

		public static void AddControlCustomisationLinkFetchHints(BusinessObjectFactory factory, ZGuid pk)
		{
			var cardTypes = new CustomisedControlTypeList();
			for (int i = 0; i < cardTypes.Count; i++)
			{
				factory.AddFetchHint(typeof(BMControlCustomisationLink), ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(BMControlCustomisationLinkSchema.Constants.FML_ParentId, BMControlCustomisationLinkSchema.Constants.TableName), pk);
			}
		}

		static List<BMControlCustomisationLink> GetPotentialLinksList(List<BMControlCustomisationLink> list, string controlType)
		{
			return (from BMControlCustomisationLink link in list
					let layout = link.CustomisedLayoutIncludingBlob
					where layout != null
					where layout.FM_ControlType == controlType
					orderby layout.FM_JobType.IsEmpty descending
					select link).ToList();
		}

		#endregion

		#region IAuditParent Mambers

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion
	}
}
