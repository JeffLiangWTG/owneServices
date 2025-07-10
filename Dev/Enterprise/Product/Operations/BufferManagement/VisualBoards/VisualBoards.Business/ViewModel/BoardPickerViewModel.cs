using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.VisualBoards.Business
{
	public class BoardPickerViewModel : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BoardPickerViewModel(CodeDescriptionPairList boards)
			: base()
		{
			Boards = boards;
		}

		[List("Boards")]
		public ZGuid BoardPK
		{
			get => boardPK;
			set
			{
				SetNonPersistentPropertyValue(BoardPKInfo, ref boardPK, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBoardPK();
				}
			}
		}
		ZGuid boardPK;

		public ZPropertyInfo BoardPKInfo => GetZPropertyInfo(nameof(BoardPK));

		public CodeDescriptionPairList Boards { get; }

		public IBMBoard GetBoard(BusinessObjectFactory factory)
		{
			return BoardPK.IsValid ? factory.Load<IBMBoard>(BoardPK) : null;
		}

		public BoardPickerViewModelValidation Validation => new BoardPickerViewModelValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		protected override ZString HumanReadableNameCore => Res.GetString("29f1c7fb-bb76-46b0-854f-80e7c0806502", "Visual Board Configuration");

		public static CodeElement GetElementForBoardList(Guid boardPk, string boardName)
		{
			return new CodeElement(new ZGuid(boardPk), boardName, boardName);
		}
	}
}
