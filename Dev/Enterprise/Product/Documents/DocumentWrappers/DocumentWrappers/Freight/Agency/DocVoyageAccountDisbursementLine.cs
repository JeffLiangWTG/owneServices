using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public class DocVoyageAccountDisbursementLine : DocBaseWrapper
	{
		DocVoyageAccountDisbursementLine(ZString group, ZString description, BusinessObjectFactory factory)
			: base(null, factory)
		{
			this.group = group;
			this.description = description;
		}

		public static DocVoyageAccountDisbursementLine New(ZString group, ZString description, BusinessObjectFactory factory)
		{
			return new DocVoyageAccountDisbursementLine(group, description, factory);
		}

		public static DocVoyageAccountDisbursementLine New(DocVoyageAccountDisbursementLine line, BusinessObjectFactory factory)
		{
			return new DocVoyageAccountDisbursementLine(line.Group, line.Description, factory);
		}

		public ZString Description
		{
			get { return description; }
		}

		public ZString Group
		{
			get { return group; }
		}

		public DocVoyageAccountDisbursementAmountCollection Amounts
		{
			get { return amounts ?? (amounts = new DocVoyageAccountDisbursementAmountCollection(Factory)); }
		}
		DocVoyageAccountDisbursementAmountCollection amounts;

		#region Implementation

		readonly ZString group;
		readonly ZString description;

		#endregion
	}
}
