# AccTransactionHeaderSubscriber Data Schema Changelog
(See [example](../ExampleChangelog.md) for how to update this document)

## [v3] = 2025-03-25
### Added
- Columns
	- `AH_SystemCreateBranch`
	- `AH_SystemCreateDepartment`

## [v2] - 2024-04-03
### Removed
- Columns
	- `AH_AutoVersion`

## [v1] - 2024-01-01

### Initial schema
- Columns
	- `AH_PK`
    - `AH_InvoiceDate`
    - `AH_OH`
    - `AH_GC`
    - `AH_SystemCreateTimeUtc`
    - `AH_SystemCreateUser`
    - `AH_SystemLastEditTimeUtc`
    - `AH_SystemLastEditUser`
    - `AH_OA_InvoiceAddressOverride`
    - `AH_OC_InvoiceContactOverride`
    - `AH_IsCancelled`
    - `AH_AutoVersion`
    - `AH_JobNumber`
- NonBizObjColumns
	- `AH_AutoVersion` (non-biz)

