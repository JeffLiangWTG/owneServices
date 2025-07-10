using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	internal partial class ClassificationLine : NonPersistentBusinessObject
	{
		public ClassificationLine(IClassificationLine1 line1, IClassificationLine2 line2, IB3SubHeader b3SubHeader)
		{
			Line1 = line1;
			Line2 = line2;
			B3SubHeader = b3SubHeader;
		}

		public ZString Description => Helper.GetDescriptionFromIClassificationLine1(Line1);

		public ZString Quantity
		{
			get { return Line2 != null ? Line2.ClassificationLineQuantity.ToString("#,###.###") : string.Empty; }
		}

		public ZString UnitOfMeasureCode
		{
			get { return Line2 != null ? Line2.UnitOfMeasureCode : ZString.Empty; }
		}

		public IClassificationLine1 Line1 { get; private set; }
		public IClassificationLine2 Line2 { get; private set; }
		public IB3SubHeader B3SubHeader { get; private set; }

		B3AndCADDocumentHelper Helper => helper ?? (helper = new B3AndCADDocumentHelper());
		B3AndCADDocumentHelper helper;
	}
}
