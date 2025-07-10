using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class OfficeCodeCollection : EuOfficeCodeCollection
{
	public OfficeCodeCollection(JobDeclaration master) : base(master)
	{
	}

	public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(OfficeCode);

	public new OfficeCode AddNew() => (OfficeCode)base.AddNew();

	public new OfficeCode this[int index] => (OfficeCode)base[index];

	public OfficeCode GetOfficeOfDestination() => ElementsAsEnumerable.FirstOrDefault(x => x.IsOfficeOfDestination);

	#region Implementation

	IEnumerable<OfficeCode> ElementsAsEnumerable => this.Cast<OfficeCode>();

	#endregion
}
