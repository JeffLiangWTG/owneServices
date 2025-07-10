
namespace Enterprise.ZArchitecture.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;

	public interface IConversionFactorLookups
	{
		CodeDescriptionPairList ConversionFactors { get; }
	}

	public class ConversionFactorLookups : ZLookups, IConversionFactorLookups
	{
		public ConversionFactorLookups(ConversionFactorViewModel parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ConversionFactors
		{
			get
			{
				return GetConversionFactors();
			}
		}

		protected virtual CodeDescriptionPairList GetConversionFactors()
		{
			var list = new CodeDescriptionPairList();

			foreach (var factor in ConversionFactor.Standard.All)
			{
				list.AddPair(factor.ToString(), factor.Description);
			}

			return list;
		}
	}
}
