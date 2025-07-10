using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.GUI.Properties;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.EConversation.GUI
{
	public partial class MessageRatingUserControl : ZUserControl
	{
		public MessageRatingUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ratingToolTip = new ToolTip();
		}
		ToolTip ratingToolTip;

		internal IConversationMessage Message { private get; set; }

		[DpiState(DpiState.Unscaled)]
		int Rating
		{
			get { return Message?.Rating ?? 0; }
		}

		const int BordersWidth = 4;

		internal void Activate(bool isActive)
		{
			if (isActive)
			{
				likeLinkLabel.Visible = true;
				dislikeLinkLabel.Visible = true;
				ratingLabel.Visible = false;

				likeLinkLabel.BackColor = BackColor;
				dislikeLinkLabel.BackColor = BackColor;

				UpdateRatingLinkLabelImages();
				UpdateRatingLinkLabelWidthAndPosition();
			}
			else
			{
				ratingLabel.Visible = true;
				likeLinkLabel.Visible = false;
				dislikeLinkLabel.Visible = false;

				if (Rating != 0)
				{
					var image = GetLinkLabelImage(Rating > 0 ? LikeImagePrefix : DislikeImagePrefix, Math.Abs(Rating), isHover: false);
					if (image != null)
					{
						ratingLabel.Image = image;
						ControlDpiScalingHelper.SetWidth(ref ratingLabel, image.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(BordersWidth), false);
						ControlDpiScalingHelper.SetHeight(ref ratingLabel, image.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(BordersWidth), false);
						ratingLabel.BackColor = BackColor;
						ControlDpiScalingHelper.SetWidth(this, ratingLabel.Width, false);
					}
				}
			}
		}

		#region Mouse Hover

		const int RatingLinkLabelToolTipVerticalPosition = 27;

		void LikeLinkLabel_MouseEnter(object sender, EventArgs e)
		{
			SetLikeLinkLabelImage(true);
			UpdateRatingLinkLabelWidthAndPosition();
			ratingToolTip.Show(Res.GetString("de86148a-aa02-455b-9a46-dcdc59e20557", "Click to like this comment"), likeLinkLabel, 0, RatingLinkLabelToolTipVerticalPosition);
		}

		void LikeLinkLabel_MouseLeave(object sender, EventArgs e)
		{
			SetLikeLinkLabelImage(false);
			UpdateRatingLinkLabelWidthAndPosition();
			ratingToolTip.Hide(likeLinkLabel);
		}

		void DislikeLinkLabel_MouseEnter(object sender, EventArgs e)
		{
			SetDisikeLinkLabelImage(true);
			UpdateRatingLinkLabelWidthAndPosition();
			ratingToolTip.Show(Res.GetString("d9b8bcce-4e3f-47de-89d2-cb946b63efdc", "Click to dislike this comment"), dislikeLinkLabel, 0, RatingLinkLabelToolTipVerticalPosition);
		}

		void DislikeLinkLabel_MouseLeave(object sender, EventArgs e)
		{
			SetDisikeLinkLabelImage(false);
			UpdateRatingLinkLabelWidthAndPosition();
			ratingToolTip.Hide(dislikeLinkLabel);
		}

		#endregion

		#region Like / Dislike Click

		void LikeLinkLabel_ClickOrDoubleClick(object sender, EventArgs e)
		{
			Message.Like();

			UpdateRatingLinkLabelImages();
			UpdateRatingLinkLabelWidthAndPosition();
		}

		void DislikeLinkLabel_ClickOrDoubleClick(object sender, EventArgs e)
		{
			Message.Dislike();

			UpdateRatingLinkLabelImages();
			UpdateRatingLinkLabelWidthAndPosition();
		}

		void UpdateRatingLinkLabelWidthAndPosition()
		{
			var likeWidthScaled = ControlDpiScalingHelper.MarkAsScaled(likeLinkLabel.Image.Width + BordersWidth);
			ControlDpiScalingHelper.SetWidth(ref likeLinkLabel, likeWidthScaled, false);

			var dislikeWidthScaled = ControlDpiScalingHelper.MarkAsScaled(dislikeLinkLabel.Image.Width + BordersWidth);
			ControlDpiScalingHelper.SetWidth(ref dislikeLinkLabel, dislikeWidthScaled, false);

			dislikeLinkLabel.Location = ControlDpiScalingHelper.NewScaledPoint(likeLinkLabel.Location.X - dislikeLinkLabel.Width, likeLinkLabel.Location.Y, false);
		}

		void UpdateRatingLinkLabelImages()
		{
			SetLikeLinkLabelImage(false);
			SetDisikeLinkLabelImage(false);
		}

		void SetLikeLinkLabelImage(bool isHover)
		{
			SetLinkLabelImage(likeLinkLabel, Math.Max(0, Rating), LikeImagePrefix, isHover);
		}

		void SetDisikeLinkLabelImage(bool isHover)
		{
			SetLinkLabelImage(dislikeLinkLabel, -Math.Min(0, Rating), DislikeImagePrefix, isHover);
		}

		void SetLinkLabelImage(KLinkLabel linkLabel, int imageIndex, string prefix, bool isHover)
		{
			var image = GetLinkLabelImage(prefix, imageIndex, isHover);
			if (image != null)
			{
				linkLabel.Image = image;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1120:DoNotGetIconsImagesFromRexOrResourcesFile", Justification = "Baseline")]
		Bitmap GetLinkLabelImage(string prefix, int imageIndex, bool isHover)
		{
			var name = isHover ?
				string.Join(ResourceNameJoiner, prefix, IconText, imageIndex, HoverText) :
				string.Join(ResourceNameJoiner, prefix, IconText, imageIndex);

			return Resources.ResourceManager.GetObject(name) as Bitmap;
		}

		#region SuppressResourceStringsCheckRegion

		const string LikeImagePrefix = "like";
		const string DislikeImagePrefix = "dislike";
		const string HoverText = "hover";
		const string IconText = "icon";
		const string ResourceNameJoiner = "_";

		#endregion

		#endregion
	}
}
