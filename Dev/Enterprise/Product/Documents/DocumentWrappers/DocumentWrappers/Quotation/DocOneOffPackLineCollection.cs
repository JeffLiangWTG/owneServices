
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOneOffPackLineCollection : DocumentWrapperCollection
	{
		public DocOneOffPackLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocOneOffPackLineCollection(RateOneOffPackLineCollection oneOffPackLineCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (var oneOffPackLine in oneOffPackLineCollection)
			{
				this.Add(DocOneOffPackLine.New(oneOffPackLine, factory));
			}
		}

		public new DocOneOffPackLine this[int index]
		{
			get { return (DocOneOffPackLine)base[index]; }
		}
	}
}
