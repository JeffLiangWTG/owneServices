using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Messaging;

namespace Enterprise.Customs.CA.Business
{
	public class ClassificationLineWrapperCollection : NonPersistentBusinessObjectCollection<ClassificationLine1Wrapper>
	{
		public ClassificationLineWrapperCollection(IB3Header header)
			: base(header.Factory)
		{
			this.header = header;
			PopulateObjects();
		}
		readonly IB3Header header;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		void PopulateObjects()
		{
			foreach (IClassificationLine1 line in header.PositiveClassificationLines)
			{
				Add(new ClassificationLine1Wrapper(line));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}
	}
}
