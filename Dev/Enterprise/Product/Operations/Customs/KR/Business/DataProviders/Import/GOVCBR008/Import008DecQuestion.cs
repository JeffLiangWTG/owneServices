using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import008DecQuestion : IImport008DecQuestion
	{
		public string QuestionID { get; set; }
		public string Answer { get; set; }

		ZString IImport008DecQuestion.QuestionID => QuestionID;
		ZString IImport008DecQuestion.Answer => Answer;

		public static class DecQuestion
		{
			public const string PossessingWeapon = "1";
			public const string PossessingDrug = "2";
			public const string PossessingLiveAnimal = "3";
			public const string PossessingEndangeredSpecies = "4";
			public const string PossessingCounterfeitItem = "5";
			public const string PossessingCommercialUse = "6";
			public const string PossessingItemBeyondDeclarationDueDate = "7";
			public const string PossessingPornography = "8";
		}
	}
}
