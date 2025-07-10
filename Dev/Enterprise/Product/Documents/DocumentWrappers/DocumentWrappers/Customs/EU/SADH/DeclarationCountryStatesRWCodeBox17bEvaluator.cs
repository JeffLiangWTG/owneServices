using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public interface IBox17bImporterStateEvaluator
	{
		ZString Evaluate();
	}

	public class DeclarationCountryStatesRWCodeBox17bEvaluator : IBox17bImporterStateEvaluator
	{
		public DeclarationCountryStatesRWCodeBox17bEvaluator(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		public ZString Evaluate()
		{
			var box17bImporterState = ZString.Empty;

			if (declaration.IsImport)
			{
				box17bImporterState = declaration.FinalDestination?.CountryStates?.RW_Code ?? ZString.Empty;
			}
			return box17bImporterState;
		}

		readonly JobDeclaration declaration;
	}
}
