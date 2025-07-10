namespace Enterprise.Accounting.CountryCompliance.Interfaces
{
	//It is private, but it is visible to country factories through InternalsVisibleTo attribute in AssebmlyInfo file.
	//This is to make this interface internal to this solution.
	interface IInstanceProvider<InstanceType>
	{
		InstanceType Get();
	}
}
