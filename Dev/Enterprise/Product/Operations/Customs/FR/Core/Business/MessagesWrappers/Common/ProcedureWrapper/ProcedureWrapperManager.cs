using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public static class ProcedureWrapperManager
	{
		public static CusProcedureWrapper NewProcedureWrapper(CusEntryHeader entryHeader, ZString actionCode, ZDateTime messageSentDate, EU.Business.ErrorCollector errorCollector)
		{
			Argument.NotNull(entryHeader, Res.GetString("b3f5c8e1-4d2b-4f8e-9b8e-1a2b3c4d5e6f", "Custom entry header cannot be null"));
			Argument.NotNull(errorCollector, Res.GetString("a1b2c3d4-e5f6-7890-ab12-cd34ef56gh78","Error collector cannot be null"));
			Argument.GreaterThanZero(entryHeader.Declaration.Invoices.Count, Res.GetString("61f3dad9-82bc-4eed-bac1-0eb85135f047", "One invoice must exist in customs procedure"));
			if (entryHeader.Declaration.IsDeltaC)
			{
				return NewProcedureWrapperCaseDeltaC(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			else
			{
				return NewProcedureWrapperCaseDeltaD(entryHeader, actionCode, messageSentDate, errorCollector);
			}
		}
		public static CusProcedureWrapper NewProcedureWrapperCaseDeltaD(CusEntryHeader entryHeader, ZString actionCode, ZDateTime messageSentDate, EU.Business.ErrorCollector errorCollector)
		{
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAL, entryHeader.Declaration.IsDeltaC).ToString() == actionCode)
			{
				return new VALProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.D2M, entryHeader.Declaration.IsDeltaC).ToString() == actionCode)
			{
				return new D2MProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANT, entryHeader.Declaration.IsDeltaC).ToString() == actionCode)
			{
				return new ANTProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.MDV, entryHeader.Declaration.IsDeltaC).ToString() == actionCode)
			{
				return new MDVProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.RPS, entryHeader.Declaration.IsDeltaC).ToString() == actionCode)
			{
				return new RPSProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.MDA, entryHeader.Declaration.IsDeltaC).ToString() == actionCode)
			{
				return new MDAProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAA, entryHeader.Declaration.IsDeltaC).ToString() == actionCode)
			{
				return new VAAProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANN, entryHeader.Declaration.IsDeltaC).ToString() == actionCode)
			{
				return new ANNProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.REC, entryHeader.Declaration.IsDeltaC).ToString() == actionCode)
			{
				return new RECProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.INV, entryHeader.Declaration.IsDeltaC).ToString() == actionCode)
			{
				return new INVProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			else
			{
				throw new NotImplementedException("CW1 does not yet support procedure wrapper type " + actionCode);
			}
		}
		public static CusProcedureWrapper NewProcedureWrapperCaseDeltaC(CusEntryHeader entryHeader, ZString actionCode, ZDateTime messageSentDate, EU.Business.ErrorCollector errorCollector)
		{
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANT).ToString() == actionCode)
			{
				return new ANTProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAL).ToString() == actionCode)
			{
				return new VALProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.MAP).ToString() == actionCode)
			{
				return new MAPProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANA).ToString() == actionCode)
			{
				return new ANAProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAA).ToString() == actionCode)
			{
				return new VAAProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.EAV).ToString() == actionCode)
			{
				return new EAVProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.INV).ToString() == actionCode)
			{
				return new INVProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.CMP).ToString() == actionCode)
			{
				return new CMPProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.RPS).ToString() == actionCode)
			{
				return new RPSProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.REC).ToString() == actionCode)
			{
				return new RECProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAR).ToString() == actionCode)
			{
				return new VARProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			if (EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.ANR).ToString() == actionCode)
			{
				return new ANRProcedureWrapper(entryHeader, actionCode, messageSentDate, errorCollector);
			}
			else
			{
				throw new NotImplementedException("CW1 does not yet support procedure wrapper type " + actionCode);
			}
		}
	}
}
