using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSlideshowValidation : AutoBMBoardSlideshowValidation
	{
		public BMBoardSlideshowValidation(AutoBMBoardSlideshow parent)
			: base(parent)
		{
		}

		public new BMBoardSlideshow Parent
		{
			get { return (BMBoardSlideshow)base.Parent; }
		}

		#region MD_Name

		protected override void CheckMD_Name()
		{
			base.CheckMD_Name();
			MandatoryValidation.CheckEntered(Parent.MD_NameInfo);
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();

			var atleastOneBoardError = Res.GetString("3CEC76B7-18F5-42D9-89F1-ABA87026525D", "You must specify at least one visual board to be included in this slide show.");
			if (Parent.BoardPivots.Count == 0)
			{
				Parent.AddRowError(atleastOneBoardError);
			}
			else
			{
				Parent.RemoveRowError(atleastOneBoardError);
			}
		}
	}
}
