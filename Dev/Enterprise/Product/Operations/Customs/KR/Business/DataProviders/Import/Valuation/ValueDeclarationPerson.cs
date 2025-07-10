using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ValueDeclarationPerson : IValueDeclarationPerson
	{
		public string DepartmentAndPosition { get; set; }
		public string Name { get; set; }
		public string TelephoneNumber { get; set; }

		ZString IValueDeclarationPerson.DepartmentAndPosition => DepartmentAndPosition;
		ZString IValueDeclarationPerson.Name => Name;
		ZString IValueDeclarationPerson.TelephoneNumber => TelephoneNumber;
	}
}
