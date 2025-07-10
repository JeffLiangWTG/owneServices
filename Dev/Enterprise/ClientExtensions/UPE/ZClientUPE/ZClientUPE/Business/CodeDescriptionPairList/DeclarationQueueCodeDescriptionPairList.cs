using CargoWise.Integration;

namespace Enterprise.Client.UPE.Business
{
	public class DeclarationQueueCodeDescriptionPairList : AutoDeclarationQueueCodeDescriptionPairList
	{
		public DeclarationQueueCodeDescriptionPairList()
		{
			MoveToLast(Codes.Classification);
			MoveToLast(Codes.Compiling);
			MoveToLast(Codes.Lodgement);
			MoveToLast(Codes.Submitted);
			MoveToLast(Codes.Unknown);
			MoveToLast(Codes.BCA);
			MoveToLast(Codes.BCO);
			MoveToLast(Codes.EIR);
			MoveToLast(Codes.Pending);
			MoveToLast(Codes.CustomsBonding);
			MoveToLast(Codes.Completed);
			ICodeDescription item = this[DefaultQueueCodeDescriptionPairList.Codes.Hold];
			Remove(item);
		}

		void MoveToLast(string code)
		{
			ICodeDescription item = this[code];
			Remove(item);
			Add(item);
		}
	}
}
