using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobEUDeclarationSetStrategy : IValueSetStrategy
	{
		public JobEUDeclarationSetStrategy(JobEUDeclaration jobEUDeclaration)
		{
			JobEUDeclaration = jobEUDeclaration;
		}

		protected JobEUDeclaration JobEUDeclaration { get; }

		void IValueSetStrategy.ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			ValueSetCore(valueThatHasChanged, oldValue);
		}

		protected virtual void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
		}
	}
}
