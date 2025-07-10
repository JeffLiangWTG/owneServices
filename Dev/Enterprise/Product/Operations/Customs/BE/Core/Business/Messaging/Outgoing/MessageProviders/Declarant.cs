using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.BE.Business;

public class Declarant : Operator, ITDeclarant
{
	public Declarant(IDocAddress address) : base(address)
	{
		ContactPerson = new StaffPerson(GlbStaff.CurrentUser);
	}

	public ZString DeclarantType { get; set; }

	public ZString DeclarantStatus
	{
		get
		{
			switch (DeclarantType)
			{
				case EU.Business.RepresentationTypeList.Codes._1Self:
					return "1";
				case EU.Business.RepresentationTypeList.Codes._2Direct:
					return "2";
				default:
					return "3";
			}
		}
	}
	public override ITContactPerson ContactPerson { get; }
}
