namespace Enterprise.ZArchitecture.Business
{
	public interface ICompanyFilterProviderContext
	{
		/// <summary>
		/// This determines whether the local or global query should come first.
		/// Example of this is for Company Tariffs and Charge Codes.
		/// If it is a Local Company Tariff, then it should search for a Local Charge Code
		/// If it is a Global Company Tariff, then it should search for a Global Charge Code
		/// </summary>
		public bool IsLocalFirst { get; }
	}
}
