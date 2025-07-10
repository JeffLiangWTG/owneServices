using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromGlbPerson : FreightWrapper
	{
		public FreightWrapperFromGlbPerson(GlbPerson person, BusinessObjectFactory factory)
			: base(person, factory)
		{
			this.person = person;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return person.PK;
		}

		protected override PersonWrapper GetPersonWrapper()
		{
			return new PersonWrapper(person, Factory);
		}

		readonly GlbPerson person;
	}
}
