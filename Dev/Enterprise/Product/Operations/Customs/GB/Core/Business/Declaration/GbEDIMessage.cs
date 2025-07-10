using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Business.Declaration
{
	[CodeProperty("MessageDescriptionForEdocs")]
	[DescriptionProperty("MessageDescriptionForEdocs")]
	public class GbEDIMessage : EDIMessage, IDocumentSupportable, Integration.Customs.GB.IGbEdiMessage
	{
		public GbEDIMessage(BusinessObjectFactory fact, System.Data.DataRow row)
			: base(fact, row)
		{
		}

		public override void OnSaving()
		{
			if (MessageNumberStrategy == null && EM_ApplicationCode != string.Empty)
			{
				MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EM_ApplicationCode);
			}
			base.OnSaving();
		}

		public new static readonly GbEDIMessageTypeDecider TypeDecider = new GbEDIMessageTypeDecider();

		public DocumentSupporter DocumentSupporter => new EdiMessageDocumentSupporter(this);

		public ZString MessageDescriptionForEdocs
		{
			get
			{
				try
				{
					var macro = GBCustomsDataRegistry.Instance.EDocsParentNameMacro.Value;
					foreach (var propertyName in new GBCustomsDataRegistry.EDocsParentNameMacroNames[] { GBCustomsDataRegistry.EDocsParentNameMacroNames.NUMBER, GBCustomsDataRegistry.EDocsParentNameMacroNames.REFERENCE, GBCustomsDataRegistry.EDocsParentNameMacroNames.SUBTYPE, GBCustomsDataRegistry.EDocsParentNameMacroNames.TYPE, GBCustomsDataRegistry.EDocsParentNameMacroNames.CODE })
					{
						ZString bizOValue = null;
						switch (propertyName)
						{
							case GBCustomsDataRegistry.EDocsParentNameMacroNames.NUMBER:
								bizOValue = EM_MessageNum;
								break;
							case GBCustomsDataRegistry.EDocsParentNameMacroNames.REFERENCE:
								bizOValue = EM_ApplicationReference;
								break;
							case GBCustomsDataRegistry.EDocsParentNameMacroNames.SUBTYPE:
								bizOValue = EM_MessageSubType;
								break;
							case GBCustomsDataRegistry.EDocsParentNameMacroNames.TYPE:
								bizOValue = EM_MessageType;
								break;
							case GBCustomsDataRegistry.EDocsParentNameMacroNames.CODE:
								bizOValue = EM_ApplicationCode;
								break;
						}

						macro = macro.Replace(string.Format("<{0}>", propertyName), bizOValue);
					}
					return macro;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return ZString.Format("{0}{1} {2} #{3}", EM_MessageType, EM_MessageSubType, EM_ApplicationReference, EM_MessageNum);
				}
			}
		}
	}
}
