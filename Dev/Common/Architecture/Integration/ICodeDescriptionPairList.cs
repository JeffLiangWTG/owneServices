using System.Collections;

namespace CargoWise.Integration
{
	public interface ICodeDescriptionPairList : IList
	{
		bool ContainsCode(object code);
		string GetDescriptionFromCode(string code);
	}
}
