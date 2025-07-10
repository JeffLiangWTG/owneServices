using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public interface IDocument9015MessageBoxProvider
	{
		bool AskIfShouldRemove9015Documents(ZString entryNumber);
	}
}
