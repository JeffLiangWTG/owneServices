namespace Enterprise.DataTransfer.Native.DB
{
	public class ColumnSchema
	{
		public string Name { get; set; }
		public string DataType { get; set; }
		public object DefaultValue { get; set; }
		public string IsNullable { get; set; }
		public object Length { get; set; }
		public object Scale { get; set; }
		public object Precision { get; set; }
	}
}