using CargoWise.Types;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DeclarationStatusProcessor
	{
		public const string Lodged = "LODGED";

		public DeclarationStatusProcessor(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		public void SetDeclarationStatus(ZString statusType)
		{
			if (declaration != null)
			{
				if (declaration.IsDrawback)
				{
					SetDeclarationStatusForDrawback(statusType);
				}
			}
		}

		void SetDeclarationStatusForDrawback(ZString statusType)
		{
			ZString result = ZString.Empty;
			switch (statusType.ToUpper())
			{
				case Lodged:
					result = CustomsEntryStatus.Lodged.Code;
					break;
			}
			declaration.JE_EntryStatus = result;
		}
	}
}
