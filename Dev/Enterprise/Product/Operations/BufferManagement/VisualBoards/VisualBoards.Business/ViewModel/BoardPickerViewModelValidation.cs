using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.VisualBoards.Business
{
	public class BoardPickerViewModelValidation : ZValidation
	{
		public BoardPickerViewModelValidation(BoardPickerViewModel viewModel)
			: base(viewModel)
		{
			Argument.NotNull(viewModel, "viewModel");
			this.viewModel = viewModel;
		}

		readonly BoardPickerViewModel viewModel;

		#region BoardPK

		public void ValidateBoardPK()
		{
			ValidateCalculatedProperty(viewModel.BoardPKInfo);
		}

		protected void CheckBoardPK()
		{
			MandatoryValidation.CheckEntered(viewModel.BoardPKInfo);
			ListValidation.ErrorIfInvalidPK(viewModel.BoardPKInfo);
		}

		#endregion

		public override Type AutoValidationType
		{
			get { return typeof(BoardPickerViewModel); }
		}

		public override void ValidateAll()
		{
			ValidateBoardPK();
		}
	}
}
