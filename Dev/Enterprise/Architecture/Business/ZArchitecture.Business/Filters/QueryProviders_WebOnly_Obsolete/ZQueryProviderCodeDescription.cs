using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class ZQueryProviderCodeDescription : CodeDescriptionPair
	{
		/// <summary>
		/// For example:
		/// new ZFilterQueryProviderCodeDescriptionPair(
		///		"LDG", "Load / Discharge",
		///		new MySubQueryContainerNoProvider(),
		///		new ZComparisonQueryProvider(JobConsol.Schema.JK_MasterBill))
		/// </summary>
		/// <param name="queryProviders">
		/// The query providers. Typically this will have 1 parameter for a number query, 2 parameters for an Organisation or Port pair.
		/// </param>
		public ZQueryProviderCodeDescription(ZString code, MultilingualString description, IQueryProvider[] queryProviders)
			: base(code.ToString(), description)
		{
			this.QueryProviders = queryProviders.ToImmutableArray();
		}

		/// <summary>
		/// For example:
		/// new ZFilterQueryProviderCodeDescriptionPair(
		///		"LDG", "Load / Discharge",
		///		new MySubQueryContainerNoProvider(),
		///		new ZComparisonQueryProvider(JobConsol.Schema.JK_MasterBill))
		/// </summary>
		/// <param name="queryProviders">
		/// The query providers. Typically this will have 1 parameter for a number query, 2 parameters for an Organisation or Port pair.
		/// </param>
		public ZQueryProviderCodeDescription(ZString code, MultilingualString description, SQLComparisonOperator @operator, IQueryProvider[] queryProviders)
			: base(code.ToString(), description)
		{
			this.QueryProviders = queryProviders.ToImmutableArray();
			this.Operator = @operator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Assume IQueryProvider is immutable.")]
		public readonly ImmutableArray<IQueryProvider> QueryProviders;
		public readonly SQLComparisonOperator Operator = SQLComparisonOperator.NotSpecified;

		public SQLComparisonOperator GetDefaultableOperator(SQLComparisonOperator @default)
		{
			if (Operator != SQLComparisonOperator.NotSpecified)
			{
				return Operator;
			}
			else
			{
				return @default;
			}
		}
	}
}
