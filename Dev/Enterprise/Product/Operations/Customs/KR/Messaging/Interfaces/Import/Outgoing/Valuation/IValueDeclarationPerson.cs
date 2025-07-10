using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IValueDeclarationPerson
	{
		ZString DepartmentAndPosition { get; }
		ZString Name { get; }
		ZString TelephoneNumber { get; }
	}
}
