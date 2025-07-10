using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	/// <summary>
	/// Used only by DebtorsAcceptGroupChargesForm for creating/updating group charges.
	/// </summary>
	public class GroupCompanyChargesForJob : NonPersistentBusinessObject
	{
		public GroupCompanyChargesForJob(Job job)
		{
			Argument.NotNull(job, nameof(job));
			Job = job;
			ChargesForDebtor = Job.GroupCompanyChargesForDebtor.BuildGroupCompanyChargeCollectionForBinding();
		}

		public Job Job { get; }

		/// <summary>
		/// The group company charges calculated at construction time.
		/// If the user applies the proposed AcceptAction this collection
		/// becomes out of date since the matches between cost and sell charges will have changed.
		/// </summary>
		public GroupCompanyChargeCollection ChargesForDebtor { get; }

		#region Autorating Options

		/// <summary>
		/// AutoratingOption - not currently used - is always invisible on the form
		/// </summary>
		[List(nameof(AutoratingOptionsList))]
		public ZString AutoratingOption
		{
			get => autoratingOption;
			set => SetNonPersistentPropertyValue(AutoratingOptionInfo, ref autoratingOption, value);
		}

		ZString autoratingOption = AutoratingOptionsCode.AutoRateCosts;

		public ZPropertyInfo AutoratingOptionInfo => GetZPropertyInfo(nameof(AutoratingOption));

		public CodeDescriptionPairList AutoratingOptionsList => Job.Factory.GetCachedValue(nameof(AutoratingOptionsList), GetAutoratingOptionsList);

		static CodeDescriptionPairList GetAutoratingOptionsList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(AutoratingOptionsCode.AutoRateCosts, Res.GetString("832dfd6f-597b-465b-a4f9-1bc1877f20f2", "Autorate Costs"));
			result.AddPair(AutoratingOptionsCode.AutoRateCostsAndRevenue, Res.GetString("1c2831e7-27dd-4ec1-b420-3e7e955be5e6", "Autorate Costs and Revenue"));
			result.AddPair(AutoratingOptionsCode.NoAutorating, Res.GetString("cd17d7ac-8812-43da-a0e3-1a0958a0b576", "Do not Autorate"));

			return result;
		}

		public static class AutoratingOptionsCode
		{
			public const string AutoRateCosts = "COS";
			public const string AutoRateCostsAndRevenue = "CAR";
			public const string NoAutorating = "NAR";
		}

		#endregion
	}
}
