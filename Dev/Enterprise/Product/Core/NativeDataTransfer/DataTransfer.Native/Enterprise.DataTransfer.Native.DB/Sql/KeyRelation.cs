using Enterprise.DataTransfer.Native.DB.Keys;

namespace Enterprise.DataTransfer.Native.DB.Sql
{
	public class KeyRelation
	{
		public KeyRelation(Key key)
		{
			FromKey = key;
			ToKey = new Key(key.ReferenceColumnDef);
		}

		public Key FromKey { get; private set; }
		public Key ToKey { get; private set; }
	}
}
