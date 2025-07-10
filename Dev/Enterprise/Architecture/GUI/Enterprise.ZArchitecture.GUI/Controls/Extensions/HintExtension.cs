using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions
{
	#region Interface

	public interface IHintExtension : IControlExtension
	{
		string ShortCaption { get; }
		string MediumCaption { get; }
		string Caption { get; set; }
		string Description { get; set; }
		ResourceStringData Data { get; }
	}

	#endregion

	public class HintExtension : ControlExtension, IHintExtension
	{
		public static string MessageForNonDefinedResourceString
		{
			get
			{
				return string.Empty;
			}
		}

		public virtual string MediumCaption
		{
			get
			{
				var data = this.Data;
				return data == null ? "" : data.MediumCaption;
			}
		}

		public virtual string ShortCaption
		{
			get
			{
				var data = this.Data;
				return data == null ? "" : data.ShortCaption;
			}
		}
		
		public virtual string Caption
		{
			get
			{
				if (!string.IsNullOrEmpty(caption))
				{
					return caption;
				}

				var data = this.Data;
				return data != null && data.Caption != null
						? ZMenuItem.StripAcceleratorKeys(data.Caption)
						: HintExtension.MessageForNonDefinedResourceString;
			}
			set { caption = value; }
		}
		string caption = string.Empty;

		public virtual string Description
		{
			get
			{
				if (!string.IsNullOrEmpty(description))
				{
					return description;
				}

				var data = this.Data;
				return data != null
							? data.FullDescription
							: HintExtension.MessageForNonDefinedResourceString;
			}
			set { description = value; }
		}
		string description = string.Empty;

		public ResourceStringData Data
		{
			get
			{
				var captionedControl = Owner.Host as IResCaptionedControl;
				if (captionedControl != null && captionedControl.CaptionResourceString != null && !captionedControl.CaptionResourceString.IsEmpty())
				{
					return captionedControl.CaptionResourceString;
				}
				else
				{
					var dataString = new ResourceStringKeyCalculator(Owner.Host).DataString;
					return dataString != null && !dataString.IsEmpty() ? dataString : null;
				}
			}
		}
	}
}
