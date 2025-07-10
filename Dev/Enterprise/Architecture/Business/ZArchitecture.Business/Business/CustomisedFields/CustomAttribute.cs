using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class CustomAttribute
	{
		public CustomAttribute(ZString key, SchemaColumn column, MultilingualString caption)
		{
			this.key = key;
			this.column = column;
			this.caption = caption;
		}

		public ZString Key
		{
			get { return key; }
		}
		readonly ZString key;

		public SchemaColumn Column
		{
			get { return column; }
		}
		readonly SchemaColumn column;

		public MultilingualString Caption
		{
			get { return caption; }
		}
		readonly MultilingualString caption;
	}
}
