using System.Data;

namespace CargoWise.Integration
{
	public interface ILastEditQuery
	{
		string GetUserNameAndTimeOfLastEditOrDeleteOfARecord(DataRow row, bool isAutoLogged);
	}
}
