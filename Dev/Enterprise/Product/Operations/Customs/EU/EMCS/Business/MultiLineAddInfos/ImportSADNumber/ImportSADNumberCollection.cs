using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class ImportSADNumberCollection<T> : ImportSADNumberCollection
		where T : ImportSADNumber
	{
		public ImportSADNumberCollection(EMCSJobDeclaration declaration) : base(declaration)
		{
			this.EnableMaxCountValidation(9
				, Res.GetString("ce093457-6ec8-49ee-b266-a58442d4aa91"
					, "There are too many Import SAD Numbers. Maximum of 9.")
				, false);
		}

		public new EMCSJobDeclaration Master => (EMCSJobDeclaration)base.Master;

		public new T this[int i] => (T)base[i];

		public new T AddNew() => (T)base.AddNew();

		public new T AddNew(Type bizObjType) => (T)base.AddNew(bizObjType);

		public override Type GetTypeOfElementsFromPK(ZGuid pK) => typeof(T);

		protected override bool AllowNewCore => base.AllowNewCore && !Master.IsMessageStatusSentOrAcknowledged;
	}
}
