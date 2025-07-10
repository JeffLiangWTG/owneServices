using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business.Testing
{
	class BrokenNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
	{
		public BrokenNumberFountainUniqueIndexFailureHandler(string uniqueIndex, BusinessObject bizObjCausingError)
			: base(uniqueIndex, bizObjCausingError)
		{
		}

		protected override INumberFountainProxy NumberFountainToFix
		{
			get { return new BrokenNumberFountain(); }
		}
	}
}
