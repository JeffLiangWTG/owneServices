using System.Linq;

namespace Enterprise.DataTransfer.Native.DB.Keys
{
	public class PolymorphicKey : ForeignKey
	{
		public PolymorphicKey(ColumnDef columnDef)
			: base(columnDef)
		{
		}

		public override Table ReferenceTable { get; set; }

		public override ColumnDef Discriminator
		{
			get
			{
				return Table.Columns.TableCodes.First();
			}
		}
	}
}