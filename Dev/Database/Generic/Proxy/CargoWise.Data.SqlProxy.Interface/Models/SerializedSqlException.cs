namespace CargoWise.Data.SqlProxy.Interface.Models;

[WTG.StaticAnalysis.Annotation.CodeAlive("SQL Over Http Connection")]
public class SerializedSqlException(int number, string message) : Exception(message)
{
	public int Number { get; set; } = number;
}
